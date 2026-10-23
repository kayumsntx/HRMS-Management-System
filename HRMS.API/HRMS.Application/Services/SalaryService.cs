using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class SalaryService : ISalaryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalaryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<EmployeeSalaryResponseDto>> GetByEmployeeIdAsync(int employeeId)
    {
        if (await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var salaries = (await _unitOfWork.Repository<EmployeeSalary>().FindAsync(s => s.EmployeeId == employeeId))
            .OrderByDescending(s => s.EffectiveFrom)
            .ToList();

        return await MapAsync(salaries);
    }

    public async Task<EmployeeSalaryResponseDto> GetCurrentAsync(int employeeId, DateTime? asOfDate)
    {
        if (await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var date = (asOfDate ?? DateTime.Now).Date;

        var salary = (await _unitOfWork.Repository<EmployeeSalary>().FindAsync(s =>
                s.EmployeeId == employeeId && s.IsActive && s.EffectiveFrom <= date &&
                (s.EffectiveTo == null || s.EffectiveTo >= date)))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault()
            ?? throw new NotFoundException($"No effective salary found for this employee on {date:yyyy-MM-dd}.");

        return (await MapAsync(new[] { salary })).First();
    }

    public async Task<EmployeeSalaryResponseDto> GetByIdAsync(int id)
    {
        var salary = await _unitOfWork.Repository<EmployeeSalary>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeSalary), id);

        return (await MapAsync(new[] { salary })).First();
    }

    public async Task<EmployeeSalaryResponseDto> CreateAsync(EmployeeSalaryCreateDto dto)
    {
        if (await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId) is null)
            throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        await ApplyPeriodRulesAsync(dto, null);

        var salary = _mapper.Map<EmployeeSalary>(dto);
        salary.EffectiveFrom = dto.EffectiveFrom.Date;
        salary.EffectiveTo = dto.EffectiveTo?.Date;

        await _unitOfWork.Repository<EmployeeSalary>().AddAsync(salary);
        await _unitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { salary })).First();
    }

    public async Task<EmployeeSalaryResponseDto> UpdateAsync(int id, EmployeeSalaryCreateDto dto)
    {
        var salary = await _unitOfWork.Repository<EmployeeSalary>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeSalary), id);

        if (dto.EmployeeId != salary.EmployeeId)
            throw new BadRequestException("The Employee of an existing salary record cannot be changed.");

                if ((dto.BasicSalary != salary.BasicSalary || dto.GrossSalary != salary.GrossSalary) &&
            await _unitOfWork.Repository<EmployeeSalaryComponent>().AnyAsync(c => c.EmployeeSalaryId == id))
            throw new BadRequestException("This salary has a component breakdown. Clear it with PUT /api/salary/{id}/components (empty list) before changing Basic/Gross, then save the breakdown again.");

        await ApplyPeriodRulesAsync(dto, id);

        _mapper.Map(dto, salary);
        salary.EffectiveFrom = dto.EffectiveFrom.Date;
        salary.EffectiveTo = dto.EffectiveTo?.Date;
        salary.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<EmployeeSalary>().Update(salary);
        await _unitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { salary })).First();
    }

    public async Task DeleteAsync(int id)
    {
        var salary = await _unitOfWork.Repository<EmployeeSalary>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeSalary), id);

        salary.IsActive = false;   // soft delete: payroll history stays traceable
        salary.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<EmployeeSalary>().Update(salary);
        await _unitOfWork.SaveChangesAsync();
    }

    // ---------------- Private Helpers ----------------

    // Rules:
    //  1. Salary periods of one employee must not overlap.
    //  2. When a new open-ended salary (no Effective To) starts after an existing open-ended one
    //     (a salary increment), the previous record is closed automatically the day before.
    private async Task ApplyPeriodRulesAsync(EmployeeSalaryCreateDto dto, int? currentId)
    {
        var newFrom = dto.EffectiveFrom.Date;
        var newTo = dto.EffectiveTo?.Date;

        var others = (await _unitOfWork.Repository<EmployeeSalary>()
                .FindAsync(s => s.EmployeeId == dto.EmployeeId && s.IsActive))
            .Where(s => s.Id != currentId)
            .ToList();

        EmployeeSalary? toClose = null;
        if (newTo == null)
        {
            toClose = others
                .Where(s => s.EffectiveTo == null && s.EffectiveFrom.Date < newFrom)
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefault();
        }

        foreach (var other in others)
        {
            if (ReferenceEquals(other, toClose))
                continue;

            var otherTo = other.EffectiveTo?.Date ?? DateTime.MaxValue.Date;
            var rangeTo = newTo ?? DateTime.MaxValue.Date;

            if (newFrom <= otherTo && rangeTo >= other.EffectiveFrom.Date)
            {
                var existingEnd = other.EffectiveTo.HasValue ? other.EffectiveTo.Value.ToString("yyyy-MM-dd") : "open";
                throw new BadRequestException(
                    $"This salary period overlaps with an existing salary record ({other.EffectiveFrom:yyyy-MM-dd} to {existingEnd}).");
            }
        }

        if (toClose != null)
        {
            toClose.EffectiveTo = newFrom.AddDays(-1);
            toClose.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Repository<EmployeeSalary>().Update(toClose);
        }
    }

    private async Task<List<EmployeeSalaryResponseDto>> MapAsync(IEnumerable<EmployeeSalary> salaries)
    {
        var list = salaries.ToList();
        var employeeIds = list.Select(s => s.EmployeeId).Distinct().ToList();

        var employees = (await _unitOfWork.Repository<Employee>().FindAsync(e => employeeIds.Contains(e.Id)))
            .ToDictionary(e => e.Id);

        var today = DateTime.Now.Date;
        var result = _mapper.Map<List<EmployeeSalaryResponseDto>>(list);

        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }

            dto.IsCurrent = dto.IsActive && dto.EffectiveFrom.Date <= today &&
                            (!dto.EffectiveTo.HasValue || dto.EffectiveTo.Value.Date >= today);
        }

        return result;
    }
}

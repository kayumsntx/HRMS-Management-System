using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class SalaryComponentService : CrudService<SalaryComponent, SalaryComponentCreateDto, SalaryComponentUpdateDto, SalaryComponentResponseDto>, ISalaryComponentService
{
    public SalaryComponentService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not SalaryComponentCreateDto d) return;

        d.ComponentType = EnumHelper.Normalize<ComponentType>(d.ComponentType, "component type");
        var name = d.ComponentName.Trim();
        var excludeId = currentId ?? 0;

        if (await UnitOfWork.Repository<SalaryComponent>().AnyAsync(c => c.ComponentName == name && c.Id != excludeId))
            throw new BadRequestException($"Salary component '{name}' already exists.");

        if (currentId.HasValue)
        {
            var existing = await UnitOfWork.Repository<SalaryComponent>().GetByIdAsync(currentId.Value);
            if (existing != null && existing.ComponentType != d.ComponentType)
            {
                var id = existing.Id;
                if (await UnitOfWork.Repository<EmployeeSalaryComponent>().AnyAsync(c => c.SalaryComponentId == id))
                    throw new BadRequestException("The type of a component that is already used in salaries cannot be changed.");
            }
        }
    }
}

public class SalaryBreakdownService : ISalaryBreakdownService
{
    private static readonly string Allowance = ComponentType.Allowance.ToString();
    private static readonly string Deduction = ComponentType.Deduction.ToString();

    private readonly IUnitOfWork _uow;

    public SalaryBreakdownService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SalaryBreakdownDto> GetAsync(int employeeSalaryId)
    {
        var salary = await LoadSalaryAsync(employeeSalaryId);
        var employee = await _uow.Repository<Employee>().GetByIdAsync(salary.EmployeeId);

        var rows = await _uow.Repository<EmployeeSalaryComponent>().FindAsync(c => c.EmployeeSalaryId == employeeSalaryId);
        var components = (await _uow.Repository<SalaryComponent>().GetAllAsync()).ToDictionary(c => c.Id);

        var lines = rows
            .Where(r => components.ContainsKey(r.SalaryComponentId))
            .Select(r => new SalaryBreakdownLineDto
            {
                SalaryComponentId = r.SalaryComponentId,
                ComponentName = components[r.SalaryComponentId].ComponentName,
                ComponentType = components[r.SalaryComponentId].ComponentType,
                Amount = r.Amount
            })
            .OrderBy(l => l.ComponentType).ThenBy(l => l.ComponentName)
            .ToList();

        return new SalaryBreakdownDto
        {
            EmployeeSalaryId = salary.Id,
            EmployeeId = salary.EmployeeId,
            EmployeeCode = employee?.EmployeeCode,
            EmployeeName = employee?.FullName,
            BasicSalary = salary.BasicSalary,
            GrossSalary = salary.GrossSalary,
            AllowanceTotal = lines.Where(l => l.ComponentType == Allowance).Sum(l => l.Amount),
            DeductionTotal = lines.Where(l => l.ComponentType == Deduction).Sum(l => l.Amount),
            Lines = lines
        };
    }

    // Replaces the whole breakdown of a salary record.
    //  - Allowances must add up to exactly (Gross - Basic).
    //  - Deductions are fixed monthly amounts (PF, tax ...); they cannot exceed the gross salary.
    public async Task<SalaryBreakdownDto> SaveAsync(int employeeSalaryId, SalaryBreakdownSaveDto dto)
    {
        var salary = await LoadSalaryAsync(employeeSalaryId);
        if (!salary.IsActive)
            throw new BadRequestException("This salary record is inactive.");

        var ids = dto.Items.Select(i => i.SalaryComponentId).ToList();
        var components = ids.Count == 0 ? new Dictionary<int, SalaryComponent>()
            : (await _uow.Repository<SalaryComponent>().FindAsync(c => ids.Contains(c.Id))).ToDictionary(c => c.Id);

        foreach (var id in ids)
        {
            if (!components.TryGetValue(id, out var component))
                throw new NotFoundException(nameof(SalaryComponent), id);

            if (!component.IsActive)
                throw new BadRequestException($"Salary component '{component.ComponentName}' is inactive.");
        }

        var allowanceTotal = dto.Items.Where(i => components[i.SalaryComponentId].ComponentType == Allowance).Sum(i => i.Amount);
        var deductionTotal = dto.Items.Where(i => components[i.SalaryComponentId].ComponentType == Deduction).Sum(i => i.Amount);
        var expectedAllowance = salary.GrossSalary - salary.BasicSalary;

        if (allowanceTotal != expectedAllowance)
            throw new BadRequestException(
                $"Allowance components must add up to {expectedAllowance:0.##} (Gross - Basic). The current total is {allowanceTotal:0.##}.");

        if (deductionTotal > salary.GrossSalary)
            throw new BadRequestException("The fixed deductions cannot exceed the gross salary.");

        var existing = await _uow.Repository<EmployeeSalaryComponent>().FindAsync(c => c.EmployeeSalaryId == employeeSalaryId);
        var incoming = dto.Items.ToDictionary(i => i.SalaryComponentId);
        var existingIds = existing.Select(r => r.SalaryComponentId).ToHashSet();

        foreach (var row in existing)
        {
            if (incoming.TryGetValue(row.SalaryComponentId, out var item))
            {
                row.Amount = item.Amount;
                row.UpdatedDate = DateTime.UtcNow;
                _uow.Repository<EmployeeSalaryComponent>().Update(row);
            }
            else
            {
                _uow.Repository<EmployeeSalaryComponent>().Delete(row);
            }
        }

        var added = dto.Items.Where(i => !existingIds.Contains(i.SalaryComponentId))
            .Select(i => new EmployeeSalaryComponent { EmployeeSalaryId = employeeSalaryId, SalaryComponentId = i.SalaryComponentId, Amount = i.Amount })
            .ToList();

        if (added.Count > 0)
            await _uow.Repository<EmployeeSalaryComponent>().AddRangeAsync(added);

        await _uow.SaveChangesAsync();
        return await GetAsync(employeeSalaryId);
    }

    private async Task<EmployeeSalary> LoadSalaryAsync(int id)
        => await _uow.Repository<EmployeeSalary>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(EmployeeSalary), id);
}

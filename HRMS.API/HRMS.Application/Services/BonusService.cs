using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

// Bonuses are added to the payroll of their month when that payroll is processed.
// A month whose payroll is Approved/Paid is locked: bonuses can no longer be added, changed or removed.
public class BonusService : EmployeeChildCrudService<Bonus, BonusCreateDto, BonusUpdateDto, BonusResponseDto>, IBonusService
{
    public BonusService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateChildAsync(object dto, int employeeId, int? currentId)
    {
        if (currentId.HasValue)
        {
            var existing = await UnitOfWork.Repository<Bonus>().GetByIdAsync(currentId.Value);
            await EnsureMonthOpenAsync(existing!.BonusYear, existing.BonusMonth);
            return;
        }

        var create = (BonusCreateDto)dto;

        if (new DateTime(create.BonusYear, create.BonusMonth, 1) > new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1))
            throw new BadRequestException("A bonus can be recorded at most for next month.");

        await EnsureMonthOpenAsync(create.BonusYear, create.BonusMonth);
    }

    protected override async Task OnDeletingAsync(Bonus entity)
        => await EnsureMonthOpenAsync(entity.BonusYear, entity.BonusMonth);

    public async Task<List<BonusResponseDto>> GetByMonthAsync(int year, int month)
    {
        if (year < 2000 || year > 2100 || month < 1 || month > 12)
            throw new BadRequestException("Please provide a valid year and month.");

        var items = await UnitOfWork.Repository<Bonus>().FindAsync(b => b.IsActive && b.BonusYear == year && b.BonusMonth == month);
        return await MapAsync(items.OrderBy(b => b.EmployeeId));
    }

    protected override async Task<List<BonusResponseDto>> MapAsync(IEnumerable<Bonus> items)
    {
        var list = items.ToList();
        var ids = list.Select(b => b.EmployeeId).Distinct().ToList();
        var employees = ids.Count == 0 ? new Dictionary<int, Employee>()
            : (await UnitOfWork.Repository<Employee>().FindAsync(e => ids.Contains(e.Id))).ToDictionary(e => e.Id);

        var result = Mapper.Map<List<BonusResponseDto>>(list);
        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }
        }
        return result;
    }

    private async Task EnsureMonthOpenAsync(int year, int month)
    {
        var payroll = (await UnitOfWork.Repository<Payroll>().FindAsync(p => p.PayrollYear == year && p.PayrollMonth == month)).FirstOrDefault();

        if (payroll != null && (payroll.Status == PayrollStatus.Approved.ToString() || payroll.Status == PayrollStatus.Paid.ToString()))
            throw new BadRequestException($"The payroll for {month}/{year} is already {payroll.Status}. Bonuses of this month are locked.");
    }
}

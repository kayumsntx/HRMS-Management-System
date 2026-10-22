using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class LeavePolicyService : CrudService<LeavePolicy, LeavePolicyCreateDto, LeavePolicyUpdateDto, LeavePolicyResponseDto>, ILeavePolicyService
{
    public LeavePolicyService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not LeavePolicyCreateDto d) return;

        var leaveType = await UnitOfWork.Repository<LeaveType>().GetByIdAsync(d.LeaveTypeId)
            ?? throw new NotFoundException(nameof(LeaveType), d.LeaveTypeId);

        if (!leaveType.IsActive)
            throw new BadRequestException("This Leave Type is inactive.");

        var excludeId = currentId ?? 0;
        var from = d.EffectiveFrom.Date;
        if (await UnitOfWork.Repository<LeavePolicy>().AnyAsync(x => x.LeaveTypeId == d.LeaveTypeId && x.EffectiveFrom == from && x.Id != excludeId))
            throw new BadRequestException("A policy for this Leave Type already starts on the same date.");
    }

    protected override async Task<List<LeavePolicyResponseDto>> MapAsync(IEnumerable<LeavePolicy> items)
    {
        var types = (await UnitOfWork.Repository<LeaveType>().GetAllAsync()).ToDictionary(t => t.Id, t => t.LeaveTypeName);
        var result = Mapper.Map<List<LeavePolicyResponseDto>>(items.ToList());
        foreach (var dto in result)
            dto.LeaveTypeName = types.GetValueOrDefault(dto.LeaveTypeId);
        return result;
    }
}

// Creates the yearly leave balances from the policies. Existing balances are never touched.
//   Opening balance = yearly days
//                     (pro-rated by the remaining months for employees who joined during the year, rounded to 0.5)
//                   + carry forward from last year (limited by the policy's max carry forward days).
public class LeaveBalanceGenerationService : ILeaveBalanceGenerationService
{
    private readonly IUnitOfWork _uow;

    public LeaveBalanceGenerationService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<LeaveBalanceGenerateResultDto> GenerateAsync(LeaveBalanceGenerateDto dto)
    {
        var year = dto.Year;
        var yearStart = new DateTime(year, 1, 1);
        var yearEnd = new DateTime(year, 12, 31);

        var activeTypes = (await _uow.Repository<LeaveType>().FindAsync(t => t.IsActive)).Select(t => t.Id).ToHashSet();

        // One policy per leave type: the latest one that is effective during the year.
        var policies = (await _uow.Repository<LeavePolicy>().FindAsync(p =>
                p.IsActive && p.EffectiveFrom <= yearEnd && (p.EffectiveTo == null || p.EffectiveTo >= yearStart)))
            .Where(p => activeTypes.Contains(p.LeaveTypeId))
            .GroupBy(p => p.LeaveTypeId)
            .Select(g => g.OrderByDescending(p => p.EffectiveFrom).First())
            .ToList();

        if (policies.Count == 0)
            throw new BadRequestException("No active leave policy is effective for this year. Please create a Leave Policy first.");

        List<Employee> employees;
        if (dto.EmployeeId.HasValue)
        {
            var employee = await _uow.Repository<Employee>().GetByIdAsync(dto.EmployeeId.Value)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.Value);

            if (!employee.IsActive)
                throw new BadRequestException("This employee is inactive.");

            employees = new List<Employee> { employee };
        }
        else
        {
            employees = await _uow.Repository<Employee>().FindAsync(e => e.IsActive && e.DateOfJoining <= yearEnd);
        }

        var existing = (await _uow.Repository<EmployeeLeaveBalance>().FindAsync(b => b.LeaveYear == year))
            .Select(b => (b.EmployeeId, b.LeaveTypeId))
            .ToHashSet();

        var previous = (await _uow.Repository<EmployeeLeaveBalance>().FindAsync(b => b.LeaveYear == year - 1))
            .ToDictionary(b => (b.EmployeeId, b.LeaveTypeId));

        var result = new LeaveBalanceGenerateResultDto { Year = year, PoliciesApplied = policies.Count };
        var toCreate = new List<EmployeeLeaveBalance>();

        foreach (var employee in employees)
        {
            var created = false;

            foreach (var policy in policies)
            {
                if (existing.Contains((employee.Id, policy.LeaveTypeId)))
                {
                    result.SkippedExisting++;
                    continue;
                }

                var entitlement = policy.YearlyDays;
                if (employee.DateOfJoining.Year == year)
                {
                    var monthsLeft = 12 - employee.DateOfJoining.Month + 1;
                    entitlement = Math.Round(policy.YearlyDays * monthsLeft / 12m * 2m, MidpointRounding.AwayFromZero) / 2m;
                }

                var carry = 0m;
                if (policy.MaxCarryForwardDays > 0 && previous.TryGetValue((employee.Id, policy.LeaveTypeId), out var last))
                    carry = Math.Min(Math.Max(0m, last.RemainingDays), policy.MaxCarryForwardDays);

                toCreate.Add(new EmployeeLeaveBalance
                {
                    EmployeeId = employee.Id,
                    LeaveTypeId = policy.LeaveTypeId,
                    LeaveYear = year,
                    OpeningBalance = entitlement + carry,
                    UsedDays = 0
                });

                result.Created++;
                created = true;
            }

            if (created)
                result.EmployeesProcessed++;
        }

        if (toCreate.Count > 0)
        {
            await _uow.Repository<EmployeeLeaveBalance>().AddRangeAsync(toCreate);
            await _uow.SaveChangesAsync();
        }

        return result;
    }
}

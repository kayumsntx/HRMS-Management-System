using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class LoanService : ILoanService
{
    // Business rule: the loan cannot exceed this many times the employee's gross salary. Change to match your policy.
    private const decimal MaxGrossSalaryMultiple = 6m;

    private static readonly string Pending = LoanStatus.Pending.ToString();
    private static readonly string Active = LoanStatus.Active.ToString();
    private static readonly string Rejected = LoanStatus.Rejected.ToString();
    private static readonly string Cancelled = LoanStatus.Cancelled.ToString();
    private static readonly string[] ApproverRoles = { "Admin", "HR" };

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _notifier;

    public LoanService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser, INotificationPublisher notifier)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    // The amount of installment number n (1-based). The last one absorbs the rounding difference.
    public static decimal InstallmentFor(Loan loan, int number)
        => number >= loan.TotalInstallments
            ? loan.LoanAmount - loan.InstallmentAmount * (loan.TotalInstallments - 1)
            : loan.InstallmentAmount;

    public async Task<List<LoanResponseDto>> GetAsync(string? status, int? employeeId)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<LoanStatus>(status, "status");

        var items = await _uow.Repository<Loan>().FindAsync(l =>
            (statusText == null || l.Status == statusText) && (!employeeId.HasValue || l.EmployeeId == employeeId.Value));

        return await MapAsync(items.OrderByDescending(l => l.Id).Take(500));
    }

    public async Task<LoanResponseDto> GetByIdAsync(int id)
        => (await MapAsync(new[] { await LoadAsync(id) })).First();

    public async Task<LoanResponseDto> CreateAsync(LoanCreateDto dto)
    {
        var employee = await _uow.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        if (!employee.IsActive)
            throw new BadRequestException("This employee is inactive.");

        var employeeId = employee.Id;
        if (await _uow.Repository<Loan>().AnyAsync(l => l.EmployeeId == employeeId && (l.Status == Pending || l.Status == Active)))
            throw new BadRequestException("This employee already has a pending or active loan.");

        var today = DateTime.Now.Date;
        var start = new DateTime(dto.StartDate.Year, dto.StartDate.Month, 1);
        if (start < new DateTime(today.Year, today.Month, 1))
            throw new BadRequestException("The first deduction month cannot be in the past.");

        var salary = (await _uow.Repository<EmployeeSalary>().FindAsync(s =>
                s.EmployeeId == employeeId && s.IsActive && s.EffectiveFrom <= today && (s.EffectiveTo == null || s.EffectiveTo >= today)))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault()
            ?? throw new BadRequestException("The employee has no current salary record, so a loan cannot be evaluated.");

        if (dto.LoanAmount > salary.GrossSalary * MaxGrossSalaryMultiple)
            throw new BadRequestException($"The loan cannot exceed {MaxGrossSalaryMultiple} x the gross salary ({salary.GrossSalary * MaxGrossSalaryMultiple:0.##}).");

        var installment = Math.Round(dto.LoanAmount / dto.TotalInstallments, 2, MidpointRounding.AwayFromZero);
        if (installment <= 0)
            throw new BadRequestException("The installment amount is too small.");

        var loan = new Loan
        {
            EmployeeId = employeeId,
            LoanAmount = dto.LoanAmount,
            InstallmentAmount = installment,
            TotalInstallments = dto.TotalInstallments,
            StartDate = start,
            Purpose = dto.Purpose?.Trim(),
            Status = Pending
        };

        if (InstallmentFor(loan, loan.TotalInstallments) <= 0)
            throw new BadRequestException("The installment plan is not valid for this amount. Try fewer installments.");

        await _uow.Repository<Loan>().AddAsync(loan);

        await _notifier.NotifyRolesAsync(ApproverRoles, NotificationCategory.Approval, "New loan request",
            $"{employee.FullName} ({employee.EmployeeCode}) requested a loan of {dto.LoanAmount:0.##}.", "/loans", _currentUser.UserId);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(loan.Id);
    }

    public async Task<LoanResponseDto> ApproveAsync(int id)
    {
        var loan = await LoadAsync(id);
        if (loan.Status != Pending)
            throw new BadRequestException($"Only a Pending loan can be approved. Current status: {loan.Status}.");

        loan.Status = Active;
        loan.ApprovedBy = _currentUser.UserId;
        loan.ApprovedDate = DateTime.UtcNow;
        loan.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Loan>().Update(loan);

        await _notifier.NotifyEmployeeAsync(loan.EmployeeId, NotificationCategory.Approval, "Loan approved",
            $"Your loan of {loan.LoanAmount:0.##} was approved. Deduction starts in {loan.StartDate:MMMM yyyy}.", "/my/loans");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<LoanResponseDto> RejectAsync(int id, ReasonDto dto)
    {
        var loan = await LoadAsync(id);
        if (loan.Status != Pending)
            throw new BadRequestException($"Only a Pending loan can be rejected. Current status: {loan.Status}.");

        loan.Status = Rejected;
        loan.RejectionReason = dto.Reason.Trim();
        loan.ApprovedBy = _currentUser.UserId;
        loan.ApprovedDate = DateTime.UtcNow;
        loan.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Loan>().Update(loan);

        await _notifier.NotifyEmployeeAsync(loan.EmployeeId, NotificationCategory.Approval, "Loan rejected",
            $"Your loan request was rejected. Reason: {dto.Reason.Trim()}", "/my/loans");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<LoanResponseDto> CancelAsync(int id)
    {
        var loan = await LoadAsync(id);
        if (loan.Status != Pending)
            throw new BadRequestException($"Only a Pending loan can be cancelled. Current status: {loan.Status}.");

        loan.Status = Cancelled;
        loan.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Loan>().Update(loan);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    private async Task<Loan> LoadAsync(int id)
        => await _uow.Repository<Loan>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(Loan), id);

    private async Task<List<LoanResponseDto>> MapAsync(IEnumerable<Loan> items)
    {
        var list = items.ToList();
        var ids = list.Select(l => l.EmployeeId).Distinct().ToList();
        var employees = ids.Count == 0 ? new Dictionary<int, Employee>()
            : (await _uow.Repository<Employee>().FindAsync(e => ids.Contains(e.Id))).ToDictionary(e => e.Id);

        var result = _mapper.Map<List<LoanResponseDto>>(list);
        foreach (var dto in result)
        {
            var loan = list.First(l => l.Id == dto.Id);

            dto.PaidAmount = loan.PaidInstallments >= loan.TotalInstallments
                ? loan.LoanAmount
                : loan.InstallmentAmount * loan.PaidInstallments;
            dto.RemainingAmount = loan.Status == Active || loan.Status == Pending ? loan.LoanAmount - dto.PaidAmount : 0m;

            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }
        }

        return result;
    }
}

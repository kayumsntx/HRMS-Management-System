using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class PayrollService : IPayrollService
{
    // ---------------- Business rules: change these if your company policy differs ----------------
    private const decimal MonthlyOvertimeDivisor = 208m;   // default OT rate = Basic / 208 x 2
    private const decimal OvertimeMultiplier = 2m;

    private static readonly string StatusDraft = PayrollStatus.Draft.ToString();
    private static readonly string StatusProcessed = PayrollStatus.Processed.ToString();
    private static readonly string StatusApproved = PayrollStatus.Approved.ToString();
    private static readonly string StatusPaid = PayrollStatus.Paid.ToString();

    private static readonly string AttAbsent = AttendanceDayStatus.Absent.ToString();
    private static readonly string AttHalfDay = AttendanceDayStatus.HalfDay.ToString();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IAttendanceService _attendanceService;
    private readonly INotificationPublisher _notifier;

    public PayrollService(
        IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IAttendanceService attendanceService,
        INotificationPublisher notifier)
    {
        _notifier = notifier;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _attendanceService = attendanceService;
    }

    // =====================================================================
    // QUERY
    // =====================================================================
    public async Task<List<PayrollResponseDto>> GetAllAsync()
    {
        var payrolls = (await _unitOfWork.Repository<Payroll>().GetAllAsync())
            .OrderByDescending(p => p.PayrollYear)
            .ThenByDescending(p => p.PayrollMonth)
            .ToList();

        var detailsByPayroll = (await _unitOfWork.Repository<PayrollDetail>().GetAllAsync())
            .GroupBy(d => d.PayrollId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return payrolls
            .Select(p => BuildResponse(p, detailsByPayroll.GetValueOrDefault(p.Id) ?? new List<PayrollDetail>()))
            .ToList();
    }

    public async Task<PayrollResponseDto> GetByIdAsync(int id)
    {
        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Payroll), id);

        var details = await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == id);
        return BuildResponse(payroll, details);
    }

    public async Task<List<PayrollDetailResponseDto>> GetDetailsAsync(int payrollId)
    {
        if (await _unitOfWork.Repository<Payroll>().GetByIdAsync(payrollId) is null)
            throw new NotFoundException(nameof(Payroll), payrollId);

        var details = await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == payrollId);
        return await MapDetailsAsync(details);
    }

    // =====================================================================
    // PROCESS (create or recalculate a month)
    // Allowed while the payroll is Draft/Processed. Approved/Paid payrolls are locked.
    // =====================================================================
    public async Task<PayrollProcessResultDto> ProcessAsync(PayrollProcessDto dto)
    {
        var monthStart = new DateTime(dto.PayrollYear, dto.PayrollMonth, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var daysInMonth = DateTime.DaysInMonth(dto.PayrollYear, dto.PayrollMonth);

        var payroll = (await _unitOfWork.Repository<Payroll>()
            .FindAsync(p => p.PayrollMonth == dto.PayrollMonth && p.PayrollYear == dto.PayrollYear))
            .FirstOrDefault();

        if (payroll != null && payroll.Status != StatusDraft && payroll.Status != StatusProcessed)
            throw new BadRequestException(
                $"Payroll for {dto.PayrollMonth}/{dto.PayrollYear} is already {payroll.Status} and cannot be processed again.");

        // ---- Load everything for the month once ----
        var attendances = await _unitOfWork.Repository<EmployeeAttendance>()
            .FindAsync(a => a.WorkingDate >= monthStart && a.WorkingDate <= monthEnd);

        if (attendances.Count == 0)
            throw new BadRequestException("No attendance records found for this month. Please process attendance first.");

        var attendanceByEmployee = attendances.GroupBy(a => a.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());

        var employees = await _unitOfWork.Repository<Employee>()
            .FindAsync(e => e.IsActive && e.DateOfJoining <= monthEnd);

        var salaryByEmployee = (await _unitOfWork.Repository<EmployeeSalary>().FindAsync(s =>
                s.IsActive && s.EffectiveFrom <= monthEnd && (s.EffectiveTo == null || s.EffectiveTo >= monthStart)))
            .GroupBy(s => s.EmployeeId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.EffectiveFrom).First());

        var leaveTypes = (await _unitOfWork.Repository<LeaveType>().GetAllAsync()).ToDictionary(l => l.Id);

        var approvedText = LeaveStatus.Approved.ToString();
        var leavesByEmployee = (await _unitOfWork.Repository<EmployeeLeave>().FindAsync(l =>
                l.Status == approvedText && l.FromDate <= monthEnd && l.ToDate >= monthStart))
            .GroupBy(l => l.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var bonusByEmployee = (await _unitOfWork.Repository<Bonus>()
                .FindAsync(b => b.IsActive && b.BonusMonth == dto.PayrollMonth && b.BonusYear == dto.PayrollYear))
            .GroupBy(b => b.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.Amount));

        var loansByEmployee = (await _unitOfWork.Repository<Loan>()
                .FindAsync(l => l.Status == LoanStatus.Active.ToString() && l.StartDate <= monthEnd))
            .GroupBy(l => l.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ---- Header ----
        if (payroll == null)
        {
            payroll = new Payroll { PayrollMonth = dto.PayrollMonth, PayrollYear = dto.PayrollYear };
            await _unitOfWork.Repository<Payroll>().AddAsync(payroll);
        }
        else
        {
            payroll.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Repository<Payroll>().Update(payroll);
        }

        payroll.Status = StatusProcessed;
        payroll.ProcessDate = DateTime.UtcNow;
        payroll.ProcessedBy = _currentUser.UserId;

        var existingDetails = payroll.Id != 0
            ? (await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == payroll.Id)).ToDictionary(d => d.EmployeeId)
            : new Dictionary<int, PayrollDetail>();

        var warnings = new List<string>();
        var processedEmployeeIds = new HashSet<int>();

        // ---- Per employee calculation ----
        foreach (var employee in employees)
        {
            var label = $"{employee.EmployeeCode} - {employee.FullName}";

            if (!salaryByEmployee.TryGetValue(employee.Id, out var salary))
            {
                warnings.Add($"{label}: no active salary record for this month, skipped.");
                continue;
            }

            if (!attendanceByEmployee.TryGetValue(employee.Id, out var empAttendance))
            {
                warnings.Add($"{label}: no attendance records for this month, skipped.");
                continue;
            }

            // Pro-rate for employees who joined during the month.
            var effectiveStart = employee.DateOfJoining.Date > monthStart ? employee.DateOfJoining.Date : monthStart;
            var payableDays = (monthEnd - effectiveStart).Days + 1;
            var fraction = (decimal)payableDays / daysInMonth;

            var basicPaid = Round(salary.BasicSalary * fraction);
            var allowancePaid = Round((salary.GrossSalary - salary.BasicSalary) * fraction);
            var perDaySalary = salary.GrossSalary / daysInMonth;

            // Deduction days = absent days + unpaid-leave days (a date is never counted twice).
            var deductionDates = new HashSet<DateTime>();

            foreach (var attendance in empAttendance.Where(a => a.DayStatus == AttAbsent && a.WorkingDate.Date >= effectiveStart))
                deductionDates.Add(attendance.WorkingDate.Date);

            if (leavesByEmployee.TryGetValue(employee.Id, out var empLeaves))
            {
                foreach (var leave in empLeaves)
                {
                    if (!leaveTypes.TryGetValue(leave.LeaveTypeId, out var leaveType) || leaveType.IsPaid)
                        continue;

                    var rangeStart = leave.FromDate.Date > effectiveStart ? leave.FromDate.Date : effectiveStart;
                    var rangeEnd = leave.ToDate.Date < monthEnd ? leave.ToDate.Date : monthEnd;

                    for (var day = rangeStart; day <= rangeEnd; day = day.AddDays(1))
                        deductionDates.Add(day);
                }
            }

            var halfDays = empAttendance.Count(a =>
                a.DayStatus == AttHalfDay && a.WorkingDate.Date >= effectiveStart && !deductionDates.Contains(a.WorkingDate.Date));

            var deduction = Round(perDaySalary * (deductionDates.Count + halfDays * 0.5m));
            deduction = Math.Min(deduction, basicPaid + allowancePaid);

            if (loansByEmployee.TryGetValue(employee.Id, out var employeeLoans))
            {
                foreach (var loan in employeeLoans.Where(l => l.PaidInstallments < l.TotalInstallments))
                    deduction += LoanService.InstallmentFor(loan, loan.PaidInstallments + 1);
            }

            // Overtime (only for overtime-eligible employees).
            var overtimeHours = employee.IsOvertimeEligible ? empAttendance.Sum(a => a.OTHours ?? 0m) : 0m;
            var overtimeRate = salary.OvertimeRatePerHour ?? Round(salary.BasicSalary / MonthlyOvertimeDivisor * OvertimeMultiplier);
            var overtimeAmount = Round(overtimeHours * overtimeRate);

            existingDetails.TryGetValue(employee.Id, out var detail);
            var isNew = detail == null;
            detail ??= new PayrollDetail { Payroll = payroll, EmployeeId = employee.Id };

            var bonus = bonusByEmployee.GetValueOrDefault(employee.Id);
            var gross = basicPaid + allowancePaid + overtimeAmount + bonus;

            detail.BasicSalary = basicPaid;
            detail.TotalAllowance = allowancePaid;
            detail.OvertimeAmount = overtimeAmount;
            detail.BonusAmount = bonus;
            detail.TotalDeduction = deduction;
            detail.GrossSalary = gross;
            detail.NetSalary = Math.Max(0m, gross - deduction);

            if (isNew)
            {
                await _unitOfWork.Repository<PayrollDetail>().AddAsync(detail);
            }
            else
            {
                detail.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Repository<PayrollDetail>().Update(detail);
            }

            processedEmployeeIds.Add(employee.Id);
        }

        if (processedEmployeeIds.Count == 0)
        {
            var reason = warnings.Count > 0 ? " " + string.Join(" | ", warnings.Take(5)) : "";
            throw new BadRequestException("No employee could be processed for this month." + reason);
        }

        // Remove details of employees that are no longer part of this run.
        foreach (var stale in existingDetails.Values.Where(d => !processedEmployeeIds.Contains(d.EmployeeId)))
            _unitOfWork.Repository<PayrollDetail>().Delete(stale);

        await _unitOfWork.SaveChangesAsync();

        var savedDetails = await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == payroll.Id);

        return new PayrollProcessResultDto
        {
            Payroll = BuildResponse(payroll, savedDetails),
            EmployeesProcessed = processedEmployeeIds.Count,
            Warnings = warnings
        };
    }

    // =====================================================================
    // ADJUST (manual bonus / deduction on a Processed payroll)
    // =====================================================================
    public async Task<PayrollDetailResponseDto> AdjustDetailAsync(int detailId, PayrollDetailAdjustDto dto)
    {
        var detail = await _unitOfWork.Repository<PayrollDetail>().GetByIdAsync(detailId)
            ?? throw new NotFoundException(nameof(PayrollDetail), detailId);

        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(detail.PayrollId)
            ?? throw new NotFoundException(nameof(Payroll), detail.PayrollId);

        if (payroll.Status != StatusProcessed)
            throw new BadRequestException($"Only a Processed payroll can be adjusted. Current status: {payroll.Status}.");

        detail.BonusAmount = dto.BonusAmount;
        detail.TotalDeduction = dto.TotalDeduction;
        detail.GrossSalary = detail.BasicSalary + detail.TotalAllowance + detail.OvertimeAmount + detail.BonusAmount;
        detail.NetSalary = Math.Max(0m, detail.GrossSalary - detail.TotalDeduction);
        detail.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<PayrollDetail>().Update(detail);
        await _unitOfWork.SaveChangesAsync();

        return (await MapDetailsAsync(new[] { detail })).First();
    }

    // =====================================================================
    // WORKFLOW: Processed -> Approved -> Paid
    // =====================================================================
    public async Task<PayrollResponseDto> ApproveAsync(int id)
    {
        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Payroll), id);

        if (payroll.Status != StatusProcessed)
            throw new BadRequestException($"Only a Processed payroll can be approved. Current status: {payroll.Status}.");

        payroll.Status = StatusApproved;
        payroll.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<Payroll>().Update(payroll);

        await NotifyPayrollAsync(payroll, "Payslip available",
            $"Your payslip for {new DateTime(payroll.PayrollYear, payroll.PayrollMonth, 1):MMMM yyyy} is now available.");

        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<PayrollResponseDto> MarkAsPaidAsync(int id)
    {
        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Payroll), id);

        if (payroll.Status != StatusApproved)
            throw new BadRequestException($"Only an Approved payroll can be marked as Paid. Current status: {payroll.Status}.");

        payroll.Status = StatusPaid;
        payroll.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<Payroll>().Update(payroll);

        await NotifyPayrollAsync(payroll, "Salary paid",
            $"Your salary for {new DateTime(payroll.PayrollYear, payroll.PayrollMonth, 1):MMMM yyyy} has been paid.");

        await AdvanceLoansAsync(payroll);

        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Payroll), id);

        if (payroll.Status != StatusDraft && payroll.Status != StatusProcessed)
            throw new BadRequestException($"An {payroll.Status} payroll cannot be deleted.");

        var details = await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == id);
        _unitOfWork.Repository<PayrollDetail>().DeleteRange(details);
        _unitOfWork.Repository<Payroll>().Delete(payroll);

        await _unitOfWork.SaveChangesAsync();
    }

    // =====================================================================
    // PAYSLIP
    // =====================================================================
    public async Task<PayslipDto> GetPayslipAsync(int payrollId, int employeeId)
    {
        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(payrollId)
            ?? throw new NotFoundException(nameof(Payroll), payrollId);

        var detail = (await _unitOfWork.Repository<PayrollDetail>()
                .FindAsync(d => d.PayrollId == payrollId && d.EmployeeId == employeeId))
            .FirstOrDefault()
            ?? throw new NotFoundException("Payslip not found for this employee in the selected payroll.");

        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        var designation = await _unitOfWork.Repository<Designation>().GetByIdAsync(employee.DesignationId);
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(employee.DepartmentId);
        var company = await _unitOfWork.Repository<Company>().GetByIdAsync(employee.CompanyId);

        var summary = await _attendanceService.GetMonthlySummaryAsync(employeeId, payroll.PayrollYear, payroll.PayrollMonth);

        return new PayslipDto
        {
            PayrollId = payroll.Id,
            PayrollMonth = payroll.PayrollMonth,
            PayrollYear = payroll.PayrollYear,
            Status = payroll.Status,

            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            DesignationName = designation?.DesignationName,
            DepartmentName = department?.DepartmentName,
            CompanyName = company?.CompanyName,
            DateOfJoining = employee.DateOfJoining,

            BasicSalary = detail.BasicSalary,
            TotalAllowance = detail.TotalAllowance,
            OvertimeAmount = detail.OvertimeAmount,
            BonusAmount = detail.BonusAmount,
            GrossSalary = detail.GrossSalary,
            TotalDeduction = detail.TotalDeduction,
            NetSalary = detail.NetSalary,

            DaysInMonth = summary.DaysInMonth,
            PresentDays = summary.PresentDays,
            LateDays = summary.LateDays,
            HalfDays = summary.HalfDays,
            AbsentDays = summary.AbsentDays,
            LeaveDays = summary.LeaveDays,
            WeekendDays = summary.WeekendDays,
            TotalWorkingHours = summary.TotalWorkingHours,
            TotalOvertimeHours = summary.TotalOvertimeHours
        };
    }

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================
    // When a payroll is paid, every active loan of an employee in that payroll moves to its next installment.
    private async Task AdvanceLoansAsync(Payroll payroll)
    {
        var monthEnd = new DateTime(payroll.PayrollYear, payroll.PayrollMonth, 1).AddMonths(1).AddDays(-1);
        var activeText = LoanStatus.Active.ToString();

        var employeeIds = (await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == payroll.Id))
            .Select(d => d.EmployeeId)
            .ToList();

        if (employeeIds.Count == 0)
            return;

        // Loans approved after this payroll was processed were not deducted, so they are not advanced.
        var loans = await _unitOfWork.Repository<Loan>().FindAsync(l =>
            l.Status == activeText && l.StartDate <= monthEnd && l.ApprovedDate != null && l.ApprovedDate <= payroll.ProcessDate &&
            employeeIds.Contains(l.EmployeeId));

        foreach (var loan in loans.Where(l => l.PaidInstallments < l.TotalInstallments))
        {
            loan.PaidInstallments += 1;
            if (loan.PaidInstallments >= loan.TotalInstallments)
                loan.Status = LoanStatus.Closed.ToString();

            loan.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Repository<Loan>().Update(loan);
        }
    }

    private async Task NotifyPayrollAsync(Payroll payroll, string title, string message)
    {
        var employeeIds = (await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == payroll.Id))
            .Select(d => d.EmployeeId)
            .ToList();

        if (employeeIds.Count == 0)
            return;

        var userIds = (await _unitOfWork.Repository<User>()
                .FindAsync(u => u.IsActive && u.EmployeeId != null && employeeIds.Contains(u.EmployeeId.Value)))
            .Select(u => u.Id)
            .ToList();

        await _notifier.NotifyUsersAsync(userIds, NotificationCategory.Payroll, title, message, "/my/payslips");
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private PayrollResponseDto BuildResponse(Payroll payroll, List<PayrollDetail> details)
    {
        var dto = _mapper.Map<PayrollResponseDto>(payroll);
        dto.TotalEmployees = details.Count;
        dto.TotalGross = details.Sum(d => d.GrossSalary);
        dto.TotalDeduction = details.Sum(d => d.TotalDeduction);
        dto.TotalNet = details.Sum(d => d.NetSalary);
        return dto;
    }

    private async Task<List<PayrollDetailResponseDto>> MapDetailsAsync(IEnumerable<PayrollDetail> details)
    {
        var employees = (await _unitOfWork.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id);
        var designations = (await _unitOfWork.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);
        var departments = (await _unitOfWork.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);

        var result = _mapper.Map<List<PayrollDetailResponseDto>>(details.ToList());
        foreach (var dto in result)
        {
            if (!employees.TryGetValue(dto.EmployeeId, out var employee))
                continue;

            dto.EmployeeCode = employee.EmployeeCode;
            dto.EmployeeName = employee.FullName;
            dto.DesignationName = designations.GetValueOrDefault(employee.DesignationId);
            dto.DepartmentName = departments.GetValueOrDefault(employee.DepartmentId);
        }

        return result.OrderBy(d => d.EmployeeCode).ToList();
    }
}

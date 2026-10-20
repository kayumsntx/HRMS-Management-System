using FluentValidation;
using HRMS.Application.DTOs.Reports;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Reports;

internal static class ReportRules
{
    public static IRuleBuilderOptions<T, string?> ValidFormat<T>(this IRuleBuilder<T, string?> rule)
        => rule.Must(v => string.IsNullOrWhiteSpace(v) ||
                          v.Equals("json", StringComparison.OrdinalIgnoreCase) ||
                          v.Equals("csv", StringComparison.OrdinalIgnoreCase))
               .WithMessage("Format must be 'json' or 'csv'.");

    public static bool ValidRange(DateTime from, DateTime to)
        => to.Date >= from.Date && (to.Date - from.Date).TotalDays <= 366;
}

public class EmployeeReportFilterValidator : AbstractValidator<EmployeeReportFilterDto>
{
    public EmployeeReportFilterValidator()
    {
        RuleFor(x => x.Format).ValidFormat();
        RuleFor(x => x)
            .Must(x => !x.JoinedFrom.HasValue || !x.JoinedTo.HasValue || x.JoinedTo.Value.Date >= x.JoinedFrom.Value.Date)
            .WithMessage("Joined To must be on or after Joined From.");
    }
}

public class MonthlyAttendanceFilterValidator : AbstractValidator<MonthlyAttendanceFilterDto>
{
    public MonthlyAttendanceFilterValidator()
    {
        RuleFor(x => x.Format).ValidFormat();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Please provide a valid year.");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
    }
}

public class DailyAttendanceFilterValidator : AbstractValidator<DailyAttendanceFilterDto>
{
    public DailyAttendanceFilterValidator()
    {
        RuleFor(x => x.Format).ValidFormat();
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.DayStatus)
            .Must(v => Enum.TryParse<AttendanceDayStatus>(v, true, out var parsed) && Enum.IsDefined(typeof(AttendanceDayStatus), parsed))
            .When(x => !string.IsNullOrWhiteSpace(x.DayStatus))
            .WithMessage("Invalid Day Status.");
    }
}

public class LateReportFilterValidator : AbstractValidator<LateReportFilterDto>
{
    public LateReportFilterValidator()
    {
        RuleFor(x => x.Format).ValidFormat();
        RuleFor(x => x.FromDate).NotEmpty().WithMessage("From Date is required.");
        RuleFor(x => x.ToDate).NotEmpty().WithMessage("To Date is required.");
        RuleFor(x => x)
            .Must(x => ReportRules.ValidRange(x.FromDate, x.ToDate))
            .WithMessage("To Date must be on or after From Date, and the range cannot exceed 366 days.");
    }
}

public class OvertimeReportFilterValidator : AbstractValidator<OvertimeReportFilterDto>
{
    public OvertimeReportFilterValidator()
    {
        RuleFor(x => x.Format).ValidFormat();
        RuleFor(x => x.FromDate).NotEmpty().WithMessage("From Date is required.");
        RuleFor(x => x.ToDate).NotEmpty().WithMessage("To Date is required.");
        RuleFor(x => x)
            .Must(x => ReportRules.ValidRange(x.FromDate, x.ToDate))
            .WithMessage("To Date must be on or after From Date, and the range cannot exceed 366 days.");
    }
}

public class LeaveReportFilterValidator : AbstractValidator<LeaveReportFilterDto>
{
    public LeaveReportFilterValidator()
    {
        RuleFor(x => x.Format).ValidFormat();
        RuleFor(x => x.FromDate).NotEmpty().WithMessage("From Date is required.");
        RuleFor(x => x.ToDate).NotEmpty().WithMessage("To Date is required.");
        RuleFor(x => x)
            .Must(x => ReportRules.ValidRange(x.FromDate, x.ToDate))
            .WithMessage("To Date must be on or after From Date, and the range cannot exceed 366 days.");
        RuleFor(x => x.Status)
            .Must(v => Enum.TryParse<LeaveStatus>(v, true, out var parsed) && Enum.IsDefined(typeof(LeaveStatus), parsed))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Invalid Status. Allowed: Pending, Approved, Rejected, Cancelled.");
    }
}

public class PayrollReportFilterValidator : AbstractValidator<PayrollReportFilterDto>
{
    public PayrollReportFilterValidator()
    {
        RuleFor(x => x.Format).ValidFormat();
        RuleFor(x => x.PayrollId).GreaterThan(0).WithMessage("Please provide a valid Payroll.");
    }
}

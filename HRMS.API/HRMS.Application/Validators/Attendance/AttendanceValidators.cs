using FluentValidation;
using HRMS.Application.DTOs.Attendance;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Attendance;

public class AttendanceProcessValidator : AbstractValidator<AttendanceProcessDto>
{
    public AttendanceProcessValidator()
    {
        RuleFor(x => x.FromDate).NotEmpty().WithMessage("From Date is required.");

        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("To Date is required.")
            .GreaterThanOrEqualTo(x => x.FromDate).WithMessage("To Date must be on or after From Date.")
            .Must(d => d.Date <= DateTime.Now.Date).WithMessage("To Date cannot be in the future.");

        RuleFor(x => x)
            .Must(x => (x.ToDate.Date - x.FromDate.Date).TotalDays < 31)
            .WithMessage("A maximum of 31 days can be processed at a time.");

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .When(x => x.EmployeeId.HasValue)
            .WithMessage("Please provide a valid Employee.");
    }
}

public class AttendanceCorrectionValidator : AbstractValidator<AttendanceCorrectionDto>
{
    public AttendanceCorrectionValidator()
    {
        RuleFor(x => x.Remarks)
            .NotEmpty().WithMessage("Remarks (reason for correction) is required.")
            .MaximumLength(450);

        RuleFor(x => x.DayStatus)
            .Must(v => Enum.TryParse<AttendanceDayStatus>(v, true, out var parsed) && Enum.IsDefined(typeof(AttendanceDayStatus), parsed))
            .When(x => !string.IsNullOrWhiteSpace(x.DayStatus))
            .WithMessage("Invalid Day Status. Allowed: Present, Absent, Late, Leave, Holiday, Weekend, HalfDay.");

        RuleFor(x => x)
            .Must(x => !x.FirstInTime.HasValue || !x.LastOutTime.HasValue || x.LastOutTime > x.FirstInTime)
            .WithMessage("Last Out Time must be after First In Time.");
    }
}

public class AttendanceFilterValidator : AbstractValidator<AttendanceFilterDto>
{
    public AttendanceFilterValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.ToDate.Value.Date >= x.FromDate.Value.Date)
            .WithMessage("To Date must be on or after From Date.");

        RuleFor(x => x.DayStatus)
            .Must(v => Enum.TryParse<AttendanceDayStatus>(v, true, out var parsed) && Enum.IsDefined(typeof(AttendanceDayStatus), parsed))
            .When(x => !string.IsNullOrWhiteSpace(x.DayStatus))
            .WithMessage("Invalid Day Status.");
    }
}

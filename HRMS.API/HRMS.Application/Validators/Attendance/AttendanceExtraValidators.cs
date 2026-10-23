using FluentValidation;
using HRMS.Application.DTOs.Attendance;

namespace HRMS.Application.Validators.Attendance;

public class ShortLeaveCreateValidator : AbstractValidator<ShortLeaveCreateDto>
{
    public ShortLeaveCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.LeaveDate).NotEmpty().WithMessage("Leave Date is required.");
        RuleFor(x => x.FromTime).Must(t => t >= TimeSpan.Zero && t < TimeSpan.FromHours(24)).WithMessage("From Time must be between 00:00 and 23:59.");
        RuleFor(x => x.ToTime).Must(t => t > TimeSpan.Zero && t < TimeSpan.FromHours(24)).WithMessage("To Time must be between 00:00 and 23:59.")
            .Must((dto, to) => to > dto.FromTime).WithMessage("To Time must be after From Time.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public class MyShortLeaveCreateValidator : AbstractValidator<MyShortLeaveCreateDto>
{
    public MyShortLeaveCreateValidator()
    {
        RuleFor(x => x.LeaveDate).NotEmpty().WithMessage("Leave Date is required.");
        RuleFor(x => x.FromTime).Must(t => t >= TimeSpan.Zero && t < TimeSpan.FromHours(24)).WithMessage("From Time must be between 00:00 and 23:59.");
        RuleFor(x => x.ToTime).Must(t => t > TimeSpan.Zero && t < TimeSpan.FromHours(24)).WithMessage("To Time must be between 00:00 and 23:59.")
            .Must((dto, to) => to > dto.FromTime).WithMessage("To Time must be after From Time.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public class AttendanceLockValidator : AbstractValidator<AttendanceLockDto>
{
    public AttendanceLockValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Please provide a valid year.");
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
        RuleFor(x => x.Remarks).MaximumLength(300);
    }
}

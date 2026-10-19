using FluentValidation;
using HRMS.Application.DTOs.Shifts;

namespace HRMS.Application.Validators.Shifts;

public class ShiftUpdateValidator : AbstractValidator<ShiftUpdateDto>
{
    public ShiftUpdateValidator()
    {
        RuleFor(x => x.ShiftName).NotEmpty().WithMessage("Shift Name is required.").MaximumLength(100);
        RuleFor(x => x.GraceTimeMinutes).InclusiveBetween(0, 180).WithMessage("Grace Time must be between 0 and 180 minutes.");
        RuleFor(x => x)
            .Must(x => x.StartTime != x.EndTime)
            .WithMessage("Start Time and End Time cannot be the same.");
        RuleFor(x => x)
            .Must(x => x.IsNightShift || x.EndTime > x.StartTime)
            .WithMessage("End Time must be after Start Time for a day shift. Mark it as Night Shift if it crosses midnight.");
    }
}

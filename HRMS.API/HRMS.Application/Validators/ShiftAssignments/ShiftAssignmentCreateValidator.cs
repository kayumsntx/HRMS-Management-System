using FluentValidation;
using HRMS.Application.DTOs.ShiftAssignments;

namespace HRMS.Application.Validators.ShiftAssignments;

public class ShiftAssignmentCreateValidator : AbstractValidator<ShiftAssignmentCreateDto>
{
    public ShiftAssignmentCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.ShiftId).GreaterThan(0).WithMessage("Please select a valid Shift.");
        RuleFor(x => x.EffectiveFrom).NotEmpty().WithMessage("Effective From is required.");
        RuleFor(x => x.EffectiveTo)
            .Must((dto, to) => to!.Value >= dto.EffectiveFrom)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective To must be on or after Effective From.");
    }
}

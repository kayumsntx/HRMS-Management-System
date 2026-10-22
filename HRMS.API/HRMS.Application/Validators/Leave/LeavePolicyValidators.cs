using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators.Leave;

public abstract class LeavePolicyRules<T> : AbstractValidator<T> where T : LeavePolicyCreateDto
{
    protected LeavePolicyRules()
    {
        RuleFor(x => x.LeaveTypeId).GreaterThan(0).WithMessage("Please select a valid Leave Type.");
        RuleFor(x => x.YearlyDays).InclusiveBetween(0, 365).WithMessage("Yearly Days must be between 0 and 365.");
        RuleFor(x => x.MaxCarryForwardDays).InclusiveBetween(0, 365).WithMessage("Carry Forward Days must be between 0 and 365.");
        RuleFor(x => x.EffectiveFrom).NotEmpty().WithMessage("Effective From is required.");
        RuleFor(x => x.EffectiveTo).Must((dto, to) => to!.Value.Date >= dto.EffectiveFrom.Date)
            .When(x => x.EffectiveTo.HasValue).WithMessage("Effective To must be on or after Effective From.");
    }
}
public class LeavePolicyCreateValidator : LeavePolicyRules<LeavePolicyCreateDto> { }
public class LeavePolicyUpdateValidator : LeavePolicyRules<LeavePolicyUpdateDto> { }

public class LeaveBalanceGenerateValidator : AbstractValidator<LeaveBalanceGenerateDto>
{
    public LeaveBalanceGenerateValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Please provide a valid year.")
            .Must(y => y <= DateTime.Now.Year + 1).WithMessage("Balances can be generated at most for next year.");
        RuleFor(x => x.EmployeeId).GreaterThan(0).When(x => x.EmployeeId.HasValue);
    }
}

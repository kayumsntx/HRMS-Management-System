using FluentValidation;
using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Validators.Payroll;

public class BonusCreateValidator : AbstractValidator<BonusCreateDto>
{
    public BonusCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.BonusType).NotEmpty().WithMessage("Bonus Type is required.").MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0).LessThan(1_000_000_000m);
        RuleFor(x => x.BonusMonth).InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
        RuleFor(x => x.BonusYear).InclusiveBetween(2000, 2100).WithMessage("Please provide a valid year.");
        RuleFor(x => x.Remarks).MaximumLength(300);
    }
}

public class BonusUpdateValidator : AbstractValidator<BonusUpdateDto>
{
    public BonusUpdateValidator()
    {
        RuleFor(x => x.BonusType).NotEmpty().WithMessage("Bonus Type is required.").MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0).LessThan(1_000_000_000m);
        RuleFor(x => x.Remarks).MaximumLength(300);
    }
}

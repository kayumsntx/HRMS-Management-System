using FluentValidation;
using HRMS.Application.Common.Helpers;
using HRMS.Application.DTOs.Payroll;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Payroll;

public abstract class SalaryComponentRules<T> : AbstractValidator<T> where T : SalaryComponentCreateDto
{
    protected SalaryComponentRules()
    {
        RuleFor(x => x.ComponentName).NotEmpty().WithMessage("Component Name is required.").MaximumLength(100);
        RuleFor(x => x.ComponentType).Must(v => EnumHelper.IsValid<ComponentType>(v)).WithMessage("Component Type must be 'Allowance' or 'Deduction'.");
    }
}
public class SalaryComponentCreateValidator : SalaryComponentRules<SalaryComponentCreateDto> { }
public class SalaryComponentUpdateValidator : SalaryComponentRules<SalaryComponentUpdateDto> { }

public class SalaryBreakdownSaveValidator : AbstractValidator<SalaryBreakdownSaveDto>
{
    public SalaryBreakdownSaveValidator()
    {
        RuleFor(x => x.Items)
            .NotNull()
            .Must(l => l == null || l.Count <= 50).WithMessage("A maximum of 50 components is allowed.")
            .Must(l => l == null || l.Select(i => i.SalaryComponentId).Distinct().Count() == l.Count).WithMessage("A component cannot be listed twice.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.SalaryComponentId).GreaterThan(0);
            item.RuleFor(i => i.Amount).GreaterThan(0).LessThan(1_000_000_000m).WithMessage("Amount must be greater than zero.");
        });
    }
}

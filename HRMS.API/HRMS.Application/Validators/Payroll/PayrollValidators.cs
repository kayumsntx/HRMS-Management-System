using FluentValidation;
using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Validators.Payroll;

public class PayrollProcessValidator : AbstractValidator<PayrollProcessDto>
{
    public PayrollProcessValidator()
    {
        RuleFor(x => x.PayrollMonth).InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
        RuleFor(x => x.PayrollYear).InclusiveBetween(2000, 2100).WithMessage("Please provide a valid year.");

        RuleFor(x => x)
            .Must(x => x.PayrollMonth < 1 || x.PayrollMonth > 12 ||
                       new DateTime(x.PayrollYear < 1 ? 1 : x.PayrollYear > 9999 ? 9999 : x.PayrollYear, x.PayrollMonth, 1) <=
                       new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
            .WithMessage("Payroll cannot be processed for a future month.");
    }
}

public class PayrollDetailAdjustValidator : AbstractValidator<PayrollDetailAdjustDto>
{
    public PayrollDetailAdjustValidator()
    {
        RuleFor(x => x.BonusAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Bonus cannot be negative.")
            .LessThan(1_000_000_000m);

        RuleFor(x => x.TotalDeduction)
            .GreaterThanOrEqualTo(0).WithMessage("Deduction cannot be negative.")
            .LessThan(1_000_000_000m);
    }
}

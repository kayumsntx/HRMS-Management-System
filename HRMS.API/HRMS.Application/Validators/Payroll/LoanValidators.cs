using FluentValidation;
using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Validators.Payroll;

public class LoanCreateValidator : AbstractValidator<LoanCreateDto>
{
    public LoanCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.LoanAmount).GreaterThan(0).LessThan(1_000_000_000m);
        RuleFor(x => x.TotalInstallments).InclusiveBetween(1, 60).WithMessage("Installments must be between 1 and 60.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start Date is required.");
        RuleFor(x => x.Purpose).MaximumLength(300);
    }
}

public class MyLoanCreateValidator : AbstractValidator<MyLoanCreateDto>
{
    public MyLoanCreateValidator()
    {
        RuleFor(x => x.LoanAmount).GreaterThan(0).LessThan(1_000_000_000m);
        RuleFor(x => x.TotalInstallments).InclusiveBetween(1, 60).WithMessage("Installments must be between 1 and 60.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start Date is required.");
        RuleFor(x => x.Purpose).MaximumLength(300);
    }
}

using FluentValidation;
using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Validators.Payroll;

public class EmployeeSalaryCreateValidator : AbstractValidator<EmployeeSalaryCreateDto>
{
    public EmployeeSalaryCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");

        RuleFor(x => x.BasicSalary)
            .GreaterThan(0).WithMessage("Basic Salary must be greater than zero.")
            .LessThan(1_000_000_000m).WithMessage("Basic Salary is too large.");

        RuleFor(x => x.GrossSalary)
            .GreaterThanOrEqualTo(x => x.BasicSalary).WithMessage("Gross Salary cannot be less than Basic Salary.")
            .LessThan(1_000_000_000m).WithMessage("Gross Salary is too large.");

        RuleFor(x => x.OvertimeRatePerHour)
            .GreaterThanOrEqualTo(0).When(x => x.OvertimeRatePerHour.HasValue)
            .WithMessage("Overtime Rate cannot be negative.");

        RuleFor(x => x.EffectiveFrom).NotEmpty().WithMessage("Effective From is required.");

        RuleFor(x => x.EffectiveTo)
            .Must((dto, to) => to!.Value.Date >= dto.EffectiveFrom.Date)
            .When(x => x.EffectiveTo.HasValue)
            .WithMessage("Effective To must be on or after Effective From.");
    }
}

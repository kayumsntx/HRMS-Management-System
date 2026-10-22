using FluentValidation;
using HRMS.Application.DTOs.EmployeeExtras;

namespace HRMS.Application.Validators.EmployeeExtras;

public abstract class EmployeeSkillRules<T> : AbstractValidator<T> where T : EmployeeSkillBase
{
    private static readonly string[] Levels = { "Beginner", "Intermediate", "Advanced", "Expert" };

    protected EmployeeSkillRules()
    {
        RuleFor(x => x.SkillCategoryId).GreaterThan(0).WithMessage("Please select a valid Skill Category.");
        RuleFor(x => x.SkillName).NotEmpty().WithMessage("Skill Name is required.").MaximumLength(150);
        RuleFor(x => x.SkillLevel)
            .Must(v => Levels.Contains(v, StringComparer.OrdinalIgnoreCase)).When(x => !string.IsNullOrWhiteSpace(x.SkillLevel))
            .WithMessage("Skill Level must be Beginner, Intermediate, Advanced or Expert.");
        RuleFor(x => x.Remarks).MaximumLength(300);
    }
}
public class EmployeeSkillCreateValidator : EmployeeSkillRules<EmployeeSkillCreateDto>
{
    public EmployeeSkillCreateValidator() { RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee."); }
}
public class EmployeeSkillUpdateValidator : EmployeeSkillRules<EmployeeSkillUpdateDto> { }

public abstract class EmployeePreviousJobRules<T> : AbstractValidator<T> where T : EmployeePreviousJobBase
{
    protected EmployeePreviousJobRules()
    {
        RuleFor(x => x.CompanyName).NotEmpty().WithMessage("Company Name is required.").MaximumLength(200);
        RuleFor(x => x.Designation).MaximumLength(150);
        RuleFor(x => x.LeavingReason).MaximumLength(300);
        RuleFor(x => x.LastSalary).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m).When(x => x.LastSalary.HasValue);
        RuleFor(x => x.ToDate).Must((dto, to) => to!.Value.Date >= dto.FromDate!.Value.Date)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue).WithMessage("To Date must be on or after From Date.");
        RuleFor(x => x.ToDate).Must(d => d!.Value.Date <= DateTime.Now.Date)
            .When(x => x.ToDate.HasValue).WithMessage("To Date cannot be in the future.");
    }
}
public class EmployeePreviousJobCreateValidator : EmployeePreviousJobRules<EmployeePreviousJobCreateDto>
{
    public EmployeePreviousJobCreateValidator() { RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee."); }
}
public class EmployeePreviousJobUpdateValidator : EmployeePreviousJobRules<EmployeePreviousJobUpdateDto> { }

public abstract class EmployeeAddressRules<T> : AbstractValidator<T> where T : EmployeeAddressBase
{
    protected EmployeeAddressRules()
    {
        RuleFor(x => x.AddressType)
            .Must(v => string.Equals(v, "Present", StringComparison.OrdinalIgnoreCase) || string.Equals(v, "Permanent", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Address Type must be 'Present' or 'Permanent'.");
        RuleFor(x => x.Village).MaximumLength(150);
        RuleFor(x => x.PostOffice).MaximumLength(100);
        RuleFor(x => x.PoliceStation).MaximumLength(100);
        RuleFor(x => x.District).MaximumLength(100);
        RuleFor(x => x.Division).MaximumLength(100);
        RuleFor(x => x.FullAddress).MaximumLength(500);
        RuleFor(x => x).Must(x => !string.IsNullOrWhiteSpace(x.FullAddress) || !string.IsNullOrWhiteSpace(x.District))
            .WithMessage("Provide at least the District or the Full Address.");
    }
}
public class EmployeeAddressCreateValidator : EmployeeAddressRules<EmployeeAddressCreateDto>
{
    public EmployeeAddressCreateValidator() { RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee."); }
}
public class EmployeeAddressUpdateValidator : EmployeeAddressRules<EmployeeAddressUpdateDto> { }

public abstract class EmployeeBankAccountRules<T> : AbstractValidator<T> where T : EmployeeBankAccountBase
{
    protected EmployeeBankAccountRules()
    {
        RuleFor(x => x.PaymentTypeId).GreaterThan(0).WithMessage("Please select a valid Payment Type.");
        RuleFor(x => x.BankId).GreaterThan(0).When(x => x.BankId.HasValue);
        RuleFor(x => x.BranchName).MaximumLength(150);
        RuleFor(x => x.AccountNumber).NotEmpty().WithMessage("Account Number is required.").MaximumLength(50)
            .Matches("^[A-Za-z0-9 -]+$").WithMessage("Account Number may contain only letters, digits, spaces and hyphens.");
    }
}
public class EmployeeBankAccountCreateValidator : EmployeeBankAccountRules<EmployeeBankAccountCreateDto>
{
    public EmployeeBankAccountCreateValidator() { RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee."); }
}
public class EmployeeBankAccountUpdateValidator : EmployeeBankAccountRules<EmployeeBankAccountUpdateDto> { }

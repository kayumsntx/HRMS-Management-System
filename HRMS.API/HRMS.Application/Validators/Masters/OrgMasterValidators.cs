using FluentValidation;
using HRMS.Application.DTOs.Masters;

namespace HRMS.Application.Validators.Masters;

public abstract class SubSectionRules<T> : AbstractValidator<T> where T : SubSectionCreateDto
{
    protected SubSectionRules()
    {
        RuleFor(x => x.SectionId).GreaterThan(0).WithMessage("Please select a valid Section.");
        RuleFor(x => x.SubSectionName).NotEmpty().WithMessage("Sub Section Name is required.").MaximumLength(150);
    }
}
public class SubSectionCreateValidator : SubSectionRules<SubSectionCreateDto> { }
public class SubSectionUpdateValidator : SubSectionRules<SubSectionUpdateDto> { }

public abstract class JobLocationRules<T> : AbstractValidator<T> where T : JobLocationCreateDto
{
    protected JobLocationRules()
    {
        RuleFor(x => x.LocationName).NotEmpty().WithMessage("Location Name is required.").MaximumLength(150);
        RuleFor(x => x.Address).MaximumLength(500);
    }
}
public class JobLocationCreateValidator : JobLocationRules<JobLocationCreateDto> { }
public class JobLocationUpdateValidator : JobLocationRules<JobLocationUpdateDto> { }

public abstract class StaffCategoryRules<T> : AbstractValidator<T> where T : StaffCategoryCreateDto
{
    protected StaffCategoryRules() { RuleFor(x => x.CategoryName).NotEmpty().WithMessage("Category Name is required.").MaximumLength(100); }
}
public class StaffCategoryCreateValidator : StaffCategoryRules<StaffCategoryCreateDto> { }
public class StaffCategoryUpdateValidator : StaffCategoryRules<StaffCategoryUpdateDto> { }

public abstract class SkillCategoryRules<T> : AbstractValidator<T> where T : SkillCategoryCreateDto
{
    protected SkillCategoryRules() { RuleFor(x => x.CategoryName).NotEmpty().WithMessage("Category Name is required.").MaximumLength(100); }
}
public class SkillCategoryCreateValidator : SkillCategoryRules<SkillCategoryCreateDto> { }
public class SkillCategoryUpdateValidator : SkillCategoryRules<SkillCategoryUpdateDto> { }

public abstract class PaymentTypeRules<T> : AbstractValidator<T> where T : PaymentTypeCreateDto
{
    protected PaymentTypeRules() { RuleFor(x => x.PaymentTypeName).NotEmpty().WithMessage("Payment Type Name is required.").MaximumLength(50); }
}
public class PaymentTypeCreateValidator : PaymentTypeRules<PaymentTypeCreateDto> { }
public class PaymentTypeUpdateValidator : PaymentTypeRules<PaymentTypeUpdateDto> { }

public abstract class BankRules<T> : AbstractValidator<T> where T : BankCreateDto
{
    protected BankRules() { RuleFor(x => x.BankName).NotEmpty().WithMessage("Bank Name is required.").MaximumLength(150); }
}
public class BankCreateValidator : BankRules<BankCreateDto> { }
public class BankUpdateValidator : BankRules<BankUpdateDto> { }

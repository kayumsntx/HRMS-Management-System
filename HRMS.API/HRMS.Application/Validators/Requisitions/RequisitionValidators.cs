using FluentValidation;
using HRMS.Application.DTOs.Requisitions;

namespace HRMS.Application.Validators.Requisitions;

public class RequisitionItemValidator : AbstractValidator<RequisitionItemDto>
{
    public RequisitionItemValidator()
    {
        RuleFor(x => x.ItemName).NotEmpty().WithMessage("Item Name is required.").MaximumLength(200);
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.").LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.Unit).MaximumLength(30);
        RuleFor(x => x.Remarks).MaximumLength(300);
    }
}

public class RequisitionCreateValidator : AbstractValidator<RequisitionCreateDto>
{
    public RequisitionCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.RequisitionTypeId).GreaterThan(0).WithMessage("Please select a valid Requisition Type.");
        RuleFor(x => x.Purpose).MaximumLength(500);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.")
            .Must(i => i == null || i.Count <= 50).WithMessage("A maximum of 50 items is allowed.");
        RuleForEach(x => x.Items).SetValidator(new RequisitionItemValidator());
    }
}

public class MyRequisitionCreateValidator : AbstractValidator<MyRequisitionCreateDto>
{
    public MyRequisitionCreateValidator()
    {
        RuleFor(x => x.RequisitionTypeId).GreaterThan(0).WithMessage("Please select a valid Requisition Type.");
        RuleFor(x => x.Purpose).MaximumLength(500);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.")
            .Must(i => i == null || i.Count <= 50).WithMessage("A maximum of 50 items is allowed.");
        RuleForEach(x => x.Items).SetValidator(new RequisitionItemValidator());
    }
}

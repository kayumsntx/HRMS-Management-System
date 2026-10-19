using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators.Leave;

public class LeaveApplyValidator : AbstractValidator<LeaveApplyDto>
{
    public LeaveApplyValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.LeaveTypeId).GreaterThan(0).WithMessage("Please select a valid Leave Type.");
        RuleFor(x => x.FromDate).NotEmpty().WithMessage("From Date is required.");
        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("To Date is required.")
            .Must((dto, to) => to.Date >= dto.FromDate.Date).WithMessage("To Date must be on or after From Date.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators.Leave;

public class LeaveRejectValidator : AbstractValidator<LeaveRejectDto>
{
    public LeaveRejectValidator()
    {
        RuleFor(x => x.RejectionReason).NotEmpty().WithMessage("Rejection Reason is required.").MaximumLength(500);
    }
}

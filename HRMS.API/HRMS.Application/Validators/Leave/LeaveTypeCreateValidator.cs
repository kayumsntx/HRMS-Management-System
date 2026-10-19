using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators.Leave;

public class LeaveTypeCreateValidator : AbstractValidator<LeaveTypeCreateDto>
{
    public LeaveTypeCreateValidator()
    {
        RuleFor(x => x.LeaveTypeName).NotEmpty().WithMessage("Leave Type Name is required.").MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}

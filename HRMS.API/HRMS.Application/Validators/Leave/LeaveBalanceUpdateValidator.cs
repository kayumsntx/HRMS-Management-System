using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators.Leave;

public class LeaveBalanceUpdateValidator : AbstractValidator<LeaveBalanceUpdateDto>
{
    public LeaveBalanceUpdateValidator()
    {
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0).WithMessage("Opening Balance cannot be negative.");
    }
}

using FluentValidation;
using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Validators.Leave;

public class LeaveBalanceCreateValidator : AbstractValidator<LeaveBalanceCreateDto>
{
    public LeaveBalanceCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.LeaveTypeId).GreaterThan(0).WithMessage("Please select a valid Leave Type.");
        RuleFor(x => x.LeaveYear).InclusiveBetween(2000, 2100).WithMessage("Please provide a valid Leave Year.");
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0).WithMessage("Opening Balance cannot be negative.");
    }
}

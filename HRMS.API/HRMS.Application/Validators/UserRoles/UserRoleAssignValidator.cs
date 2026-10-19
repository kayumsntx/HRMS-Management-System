using FluentValidation;
using HRMS.Application.DTOs.UserRoles;

namespace HRMS.Application.Validators.UserRoles;

public class UserRoleAssignValidator : AbstractValidator<UserRoleAssignDto>
{
    public UserRoleAssignValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Please select a valid User.");
        RuleFor(x => x.RoleId).GreaterThan(0).WithMessage("Please select a valid Role.");
    }
}

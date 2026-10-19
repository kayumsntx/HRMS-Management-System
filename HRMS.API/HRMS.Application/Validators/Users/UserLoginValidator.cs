using FluentValidation;
using HRMS.Application.DTOs.Users;

namespace HRMS.Application.Validators.Users;

public class UserLoginValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("User Name is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

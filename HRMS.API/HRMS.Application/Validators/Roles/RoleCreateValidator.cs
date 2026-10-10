using FluentValidation;
using HRMS.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Roles
{
    public class RoleCreateValidator : AbstractValidator<RoleCreateDto>
    {
        public RoleCreateValidator()
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage("Role Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(300);
        }
    }
}

using FluentValidation;
using HRMS.Application.DTOs.EmployeeStatuses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.EmployeeStatuses
{
    public class EmployeeStatusCreateValidator : AbstractValidator<EmployeeStatusCreateDto>
    {
        public EmployeeStatusCreateValidator()
        {
            RuleFor(x => x.StatusName)
                .NotEmpty().WithMessage("Status Name is required.")
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MaximumLength(300);
        }
    }
}

using FluentValidation;
using HRMS.Application.DTOs.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Employees
{
    public class EmployeeUpdateValidator : AbstractValidator<EmployeeUpdateDto>
    {
        public EmployeeUpdateValidator()
        {
            //RuleFor(x => x.EmployeeCode)
            //    .NotEmpty().WithMessage("Employee Code is required.")
            //    .MaximumLength(20);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(150);

            RuleFor(x => x.DateOfJoining)
                .NotEmpty().WithMessage("Date of Joining is required.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Please provide a valid Email.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.EmployeeStatusId)
                .GreaterThan(0).WithMessage("Please select a valid Employee Status.");

            RuleFor(x => x.DesignationId)
                .GreaterThan(0).WithMessage("Please select a valid Designation.");

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Please select a valid Company.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Please select a valid Department.");
        }
    }
}

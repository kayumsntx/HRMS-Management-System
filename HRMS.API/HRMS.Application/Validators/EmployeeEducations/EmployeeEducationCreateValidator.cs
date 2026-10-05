using FluentValidation;
using HRMS.Application.DTOs.EmployeeEducations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.EmployeeEducations
{
    public class EmployeeEducationCreateValidator : AbstractValidator<EmployeeEducationCreateDto>
    {
        public EmployeeEducationCreateValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Please select a valid Employee.");

            RuleFor(x => x.Degree)
                .NotEmpty().WithMessage("Degree is required.")
                .MaximumLength(100);

            RuleFor(x => x.Institution)
                .NotEmpty().WithMessage("Institution is required.")
                .MaximumLength(200);

            RuleFor(x => x.PassingYear)
                .InclusiveBetween(1950, DateTime.UtcNow.Year)
                .When(x => x.PassingYear.HasValue)
                .WithMessage("Please provide a valid Passing Year.");
        }
    }
}

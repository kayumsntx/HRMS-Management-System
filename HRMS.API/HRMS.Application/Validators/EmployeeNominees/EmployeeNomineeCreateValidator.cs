using FluentValidation;
using HRMS.Application.DTOs.EmployeeNominees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.EmployeeNominees
{
    public class EmployeeNomineeCreateValidator : AbstractValidator<EmployeeNomineeCreateDto>
    {
        public EmployeeNomineeCreateValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Please select a valid Employee.");

            RuleFor(x => x.NomineeName)
                .NotEmpty().WithMessage("Nominee Name is required.")
                .MaximumLength(150);

            RuleFor(x => x.Relationship)
                .NotEmpty().WithMessage("Relationship is required.")
                .MaximumLength(50);

            RuleFor(x => x.SharePercentage)
                .InclusiveBetween(0, 100)
                .When(x => x.SharePercentage.HasValue)
                .WithMessage("Share Percentage must be between 0 and 100.");

            RuleFor(x => x.MobileNumber)
                .MaximumLength(20);
        }
    }
}

using FluentValidation.Validators;
using FluentValidation;
using HRMS.Application.DTOs.Companies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Companies
{
    public class CompanyCreateValidator: AbstractValidator<CompanyCreateDto>
    {
        public CompanyCreateValidator()
        {
            RuleFor(x => x.CompanyCode)
             .NotEmpty().WithMessage("Company Code Required")
             .MaximumLength(20).WithMessage("Company Code Max 20 Character");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company Name Required")
                .MaximumLength(200).WithMessage("Company Name Max 200 Character");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Input valid Email")
                .When(x => !string.IsNullOrEmpty(x.Email));
            RuleFor(x => x.Phone)
                .Length(11)
                .WithMessage("The phone number must be exactly {Length} digits long.");

        }
    }
}

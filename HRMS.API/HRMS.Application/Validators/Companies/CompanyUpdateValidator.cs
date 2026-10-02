using FluentValidation;
using HRMS.Application.DTOs.Companies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Companies
{
    public class CompanyUpdateValidator : AbstractValidator<CompanyUpdateDto>
    {
        public CompanyUpdateValidator()
        {
            RuleFor(x => x.CompanyCode)
            .NotEmpty().WithMessage("Company Code Required")
            .MaximumLength(20);

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company Name Required")
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Input valid Email")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}

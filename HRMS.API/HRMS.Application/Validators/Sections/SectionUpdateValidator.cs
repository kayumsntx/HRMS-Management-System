using FluentValidation;
using HRMS.Application.DTOs.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Sections
{
    public class SectionUpdateValidator : AbstractValidator<SectionUpdateDto>
    {
        public SectionUpdateValidator()
        {
            RuleFor(x => x.SectionName)
                .NotEmpty().WithMessage("Section Name is required.")
                .MaximumLength(150);

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Please select a valid Department.");
        }
    }
}

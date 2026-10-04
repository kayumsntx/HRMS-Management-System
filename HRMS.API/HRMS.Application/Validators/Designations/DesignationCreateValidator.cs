using FluentValidation;
using HRMS.Application.DTOs.Designations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Designations
{
    public class DesignationCreateValidator : AbstractValidator<DesignationCreateDto>
    {
        public DesignationCreateValidator()
        {
            RuleFor(x => x.DesignationName)
                .NotEmpty().WithMessage("Designation Name is required.")
                .MaximumLength(150);
        }
    }
}

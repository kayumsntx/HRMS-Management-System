using FluentValidation;
using HRMS.Application.DTOs.Grades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Grades
{
    public class GradeCreateValidator : AbstractValidator<GradeCreateDto>
    {
        public GradeCreateValidator()
        {
            RuleFor(x => x.GradeName)
                .NotEmpty().WithMessage("Grade Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }
}

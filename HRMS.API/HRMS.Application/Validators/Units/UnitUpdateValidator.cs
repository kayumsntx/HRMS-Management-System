using FluentValidation;
using HRMS.Application.DTOs.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Validators.Units
{
    public class UnitUpdateValidator : AbstractValidator<UnitUpdateDto>
    {
        public UnitUpdateValidator()
        {
            RuleFor(x => x.UnitName)
            .NotEmpty().WithMessage("Unit Name Required")
            .MaximumLength(150);

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("Select Correct Company");
        }
    }
}

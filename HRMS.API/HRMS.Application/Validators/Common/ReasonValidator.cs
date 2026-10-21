using FluentValidation;
using HRMS.Application.DTOs.Common;

namespace HRMS.Application.Validators.Common;

public class ReasonValidator : AbstractValidator<ReasonDto>
{
    public ReasonValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.").MaximumLength(500);
    }
}

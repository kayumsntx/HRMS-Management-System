using FluentValidation;
using HRMS.Application.DTOs.IdCards;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.IdCards;

public class IdCardIssueValidator : AbstractValidator<IdCardIssueDto>
{
    public IdCardIssueValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");

        RuleFor(x => x.ExpiryDate)
            .Must(d => d!.Value.Date > DateTime.Now.Date)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry Date must be in the future.");
    }
}

public class IdCardReissueValidator : AbstractValidator<IdCardReissueDto>
{
    public IdCardReissueValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Re-issue reason is required.").MaximumLength(300);

        RuleFor(x => x.ExpiryDate)
            .Must(d => d!.Value.Date > DateTime.Now.Date)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry Date must be in the future.");
    }
}

public class IdCardStatusValidator : AbstractValidator<IdCardStatusDto>
{
    public IdCardStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(v => Enum.TryParse<IdCardStatus>(v, true, out var parsed) && Enum.IsDefined(typeof(IdCardStatus), parsed))
            .WithMessage("Invalid Status. Allowed: Lost, Cancelled, Expired.");
    }
}

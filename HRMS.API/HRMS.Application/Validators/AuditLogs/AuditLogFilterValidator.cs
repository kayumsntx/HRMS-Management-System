using FluentValidation;
using HRMS.Application.DTOs.AuditLogs;

namespace HRMS.Application.Validators.AuditLogs;

public class AuditLogFilterValidator : AbstractValidator<AuditLogFilterDto>
{
    public AuditLogFilterValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.ToDate.Value.Date >= x.FromDate.Value.Date)
            .WithMessage("To Date must be on or after From Date.");

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || (x.ToDate.Value.Date - x.FromDate.Value.Date).TotalDays <= 31)
            .WithMessage("A maximum of 31 days can be searched at a time.");
    }
}

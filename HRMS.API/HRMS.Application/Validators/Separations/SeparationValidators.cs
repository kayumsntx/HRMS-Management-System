using FluentValidation;
using HRMS.Application.Common.Helpers;
using HRMS.Application.DTOs.Separations;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Separations;

public class SeparationCreateValidator : AbstractValidator<SeparationCreateDto>
{
    public SeparationCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.SeparationType).Must(v => EnumHelper.IsValid<SeparationType>(v))
            .WithMessage("Invalid Separation Type. Allowed: Resignation, Termination, Retirement, Other.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.").MaximumLength(500);
        RuleFor(x => x.LastWorkingDate).Must((dto, d) => d!.Value.Date >= (dto.RequestDate ?? DateTime.Now).Date)
            .When(x => x.LastWorkingDate.HasValue).WithMessage("Last Working Date cannot be before the Request Date.");
    }
}

public class MySeparationCreateValidator : AbstractValidator<MySeparationCreateDto>
{
    public MySeparationCreateValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.").MaximumLength(500);
        RuleFor(x => x.LastWorkingDate).Must(d => d!.Value.Date >= DateTime.Now.Date)
            .When(x => x.LastWorkingDate.HasValue).WithMessage("Last Working Date cannot be in the past.");
    }
}

public class SeparationApproveValidator : AbstractValidator<SeparationApproveDto>
{
    public SeparationApproveValidator()
    {
        RuleFor(x => x.LastWorkingDate).NotEmpty().WithMessage("Last Working Date is required.");
    }
}

public class ExitInterviewValidator : AbstractValidator<ExitInterviewDto>
{
    public ExitInterviewValidator()
    {
        RuleFor(x => x.InterviewDate).NotEmpty().WithMessage("Interview Date is required.");
        RuleFor(x => x.InterviewerEmployeeId).GreaterThan(0).When(x => x.InterviewerEmployeeId.HasValue);
        RuleFor(x => x.Feedback).MaximumLength(2000);
    }
}

public class ClearanceCreateValidator : AbstractValidator<ClearanceCreateDto>
{
    public ClearanceCreateValidator()
    {
        RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Please select a valid Department.");
        RuleFor(x => x.DueAmount).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

public class ClearanceUpdateValidator : AbstractValidator<ClearanceUpdateDto>
{
    public ClearanceUpdateValidator()
    {
        RuleFor(x => x.DueAmount).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

public class SettlementSaveValidator : AbstractValidator<SettlementSaveDto>
{
    public SettlementSaveValidator()
    {
        RuleFor(x => x.LeaveEncashment).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m);
        RuleFor(x => x.OtherPayable).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m);
        RuleFor(x => x.OtherDeduction).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m);
    }
}

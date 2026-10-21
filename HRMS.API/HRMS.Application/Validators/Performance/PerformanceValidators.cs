using FluentValidation;
using HRMS.Application.Common.Helpers;
using HRMS.Application.DTOs.Performance;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Performance;

public class GoalCreateValidator : AbstractValidator<GoalCreateDto>
{
    public GoalCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.PerformancePeriodId).GreaterThan(0).WithMessage("Please select a valid Performance Period.");
        RuleFor(x => x.GoalTitle).NotEmpty().WithMessage("Goal Title is required.").MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TargetValue).GreaterThan(0).LessThan(1_000_000_000m).When(x => x.TargetValue.HasValue);
    }
}

public class GoalUpdateValidator : AbstractValidator<GoalUpdateDto>
{
    public GoalUpdateValidator()
    {
        RuleFor(x => x.GoalTitle).NotEmpty().WithMessage("Goal Title is required.").MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.TargetValue).GreaterThan(0).LessThan(1_000_000_000m).When(x => x.TargetValue.HasValue);
        RuleFor(x => x.AchievedValue).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m).When(x => x.AchievedValue.HasValue);
        RuleFor(x => x.Status).Must(v => EnumHelper.IsValid<GoalStatus>(v)).WithMessage("Invalid Status. Allowed: Open, InProgress, Completed, Cancelled.");
    }
}

public class GoalProgressValidator : AbstractValidator<GoalProgressDto>
{
    public GoalProgressValidator()
    {
        RuleFor(x => x.AchievedValue).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m);
    }
}

public class KpiScoreValidator : AbstractValidator<KpiScoreDto>
{
    public KpiScoreValidator()
    {
        RuleFor(x => x.KpiId).GreaterThan(0).WithMessage("Please select a valid KPI.");
        RuleFor(x => x.Score).InclusiveBetween(0, 100).WithMessage("Score must be between 0 and 100.");
        RuleFor(x => x.Comments).MaximumLength(500);
    }
}

public class EvaluationCreateValidator : AbstractValidator<EvaluationCreateDto>
{
    public EvaluationCreateValidator()
    {
        RuleFor(x => x.EmployeeId).GreaterThan(0).WithMessage("Please select a valid Employee.");
        RuleFor(x => x.PerformancePeriodId).GreaterThan(0).WithMessage("Please select a valid Performance Period.");
        RuleFor(x => x.Comments).MaximumLength(1000);
        RuleFor(x => x.KpiScores)
            .Must(l => l == null || l.Select(s => s.KpiId).Distinct().Count() == l.Count).WithMessage("A KPI cannot be scored twice.")
            .Must(l => l == null || l.Count <= 50).WithMessage("A maximum of 50 KPI scores is allowed.");
        RuleForEach(x => x.KpiScores).SetValidator(new KpiScoreValidator());
    }
}

public class EvaluationUpdateValidator : AbstractValidator<EvaluationUpdateDto>
{
    public EvaluationUpdateValidator()
    {
        RuleFor(x => x.Comments).MaximumLength(1000);
        RuleFor(x => x.KpiScores)
            .Must(l => l == null || l.Select(s => s.KpiId).Distinct().Count() == l.Count).WithMessage("A KPI cannot be scored twice.")
            .Must(l => l == null || l.Count <= 50).WithMessage("A maximum of 50 KPI scores is allowed.");
        RuleForEach(x => x.KpiScores).SetValidator(new KpiScoreValidator());
    }
}

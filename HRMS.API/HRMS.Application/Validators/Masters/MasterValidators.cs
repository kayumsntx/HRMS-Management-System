using FluentValidation;
using HRMS.Application.DTOs.Masters;

namespace HRMS.Application.Validators.Masters;

// One abstract rule set per master; the concrete Create/Update validators below reuse it.
public abstract class RequisitionTypeRules<T> : AbstractValidator<T> where T : RequisitionTypeCreateDto
{
    protected RequisitionTypeRules()
    {
        RuleFor(x => x.TypeName).NotEmpty().WithMessage("Type Name is required.").MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(300);
    }
}
public class RequisitionTypeCreateValidator : RequisitionTypeRules<RequisitionTypeCreateDto> { }
public class RequisitionTypeUpdateValidator : RequisitionTypeRules<RequisitionTypeUpdateDto> { }

public abstract class TrainingProgramRules<T> : AbstractValidator<T> where T : TrainingProgramCreateDto
{
    protected TrainingProgramRules()
    {
        RuleFor(x => x.ProgramName).NotEmpty().WithMessage("Program Name is required.").MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DurationHours).InclusiveBetween(1, 2000).When(x => x.DurationHours.HasValue).WithMessage("Duration must be between 1 and 2000 hours.");
    }
}
public class TrainingProgramCreateValidator : TrainingProgramRules<TrainingProgramCreateDto> { }
public class TrainingProgramUpdateValidator : TrainingProgramRules<TrainingProgramUpdateDto> { }

public abstract class TrainerRules<T> : AbstractValidator<T> where T : TrainerCreateDto
{
    protected TrainerRules()
    {
        RuleFor(x => x.TrainerName).NotEmpty().WithMessage("Trainer Name is required.").MaximumLength(150);
        RuleFor(x => x.EmployeeId).GreaterThan(0).When(x => x.EmployeeId.HasValue);
        RuleFor(x => x.Organization).MaximumLength(200);
        RuleFor(x => x.Mobile).MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Please provide a valid Email.").MaximumLength(150);
    }
}
public class TrainerCreateValidator : TrainerRules<TrainerCreateDto> { }
public class TrainerUpdateValidator : TrainerRules<TrainerUpdateDto> { }

public abstract class PerformancePeriodRules<T> : AbstractValidator<T> where T : PerformancePeriodCreateDto
{
    protected PerformancePeriodRules()
    {
        RuleFor(x => x.PeriodName).NotEmpty().WithMessage("Period Name is required.").MaximumLength(100);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().Must((dto, end) => end.Date >= dto.StartDate.Date).WithMessage("End Date must be on or after Start Date.");
    }
}
public class PerformancePeriodCreateValidator : PerformancePeriodRules<PerformancePeriodCreateDto> { }
public class PerformancePeriodUpdateValidator : PerformancePeriodRules<PerformancePeriodUpdateDto> { }

public abstract class KpiRules<T> : AbstractValidator<T> where T : KpiCreateDto
{
    protected KpiRules()
    {
        RuleFor(x => x.KpiName).NotEmpty().WithMessage("KPI Name is required.").MaximumLength(150);
        RuleFor(x => x.Weight).InclusiveBetween(0, 100).WithMessage("Weight must be between 0 and 100.");
        RuleFor(x => x.DepartmentId).GreaterThan(0).When(x => x.DepartmentId.HasValue);
        RuleFor(x => x.DesignationId).GreaterThan(0).When(x => x.DesignationId.HasValue);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
public class KpiCreateValidator : KpiRules<KpiCreateDto> { }
public class KpiUpdateValidator : KpiRules<KpiUpdateDto> { }

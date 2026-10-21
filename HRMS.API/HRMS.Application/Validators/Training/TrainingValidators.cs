using FluentValidation;
using HRMS.Application.DTOs.Training;

namespace HRMS.Application.Validators.Training;

public class TrainingScheduleCreateValidator : AbstractValidator<TrainingScheduleCreateDto>
{
    public TrainingScheduleCreateValidator()
    {
        RuleFor(x => x.TrainingProgramId).GreaterThan(0).WithMessage("Please select a valid Training Program.");
        RuleFor(x => x.TrainerId).GreaterThan(0).When(x => x.TrainerId.HasValue);
        RuleFor(x => x.StartDateTime).NotEmpty().WithMessage("Start Date is required.");
        RuleFor(x => x.EndDateTime).NotEmpty().Must((dto, end) => end > dto.StartDateTime).WithMessage("End must be after Start.");
        RuleFor(x => x).Must(x => (x.EndDateTime - x.StartDateTime).TotalDays <= 366).WithMessage("A training cannot be longer than one year.");
        RuleFor(x => x.Venue).MaximumLength(200);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m).When(x => x.Cost.HasValue);
    }
}

public class TrainingScheduleUpdateValidator : AbstractValidator<TrainingScheduleUpdateDto>
{
    public TrainingScheduleUpdateValidator()
    {
        RuleFor(x => x.TrainerId).GreaterThan(0).When(x => x.TrainerId.HasValue);
        RuleFor(x => x.StartDateTime).NotEmpty().WithMessage("Start Date is required.");
        RuleFor(x => x.EndDateTime).NotEmpty().Must((dto, end) => end > dto.StartDateTime).WithMessage("End must be after Start.");
        RuleFor(x => x).Must(x => (x.EndDateTime - x.StartDateTime).TotalDays <= 366).WithMessage("A training cannot be longer than one year.");
        RuleFor(x => x.Venue).MaximumLength(200);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).LessThan(1_000_000_000m).When(x => x.Cost.HasValue);
    }
}

public class TrainingEnrollValidator : AbstractValidator<TrainingEnrollDto>
{
    public TrainingEnrollValidator()
    {
        RuleFor(x => x.EmployeeIds)
            .NotEmpty().WithMessage("At least one employee is required.")
            .Must(l => l == null || l.Count <= 200).WithMessage("A maximum of 200 employees can be enrolled at a time.")
            .Must(l => l == null || l.All(i => i > 0)).WithMessage("Employee ids must be greater than zero.");
    }
}

public class TrainingAttendanceMarkValidator : AbstractValidator<TrainingAttendanceMarkDto>
{
    public TrainingAttendanceMarkValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one attendance item is required.")
            .Must(l => l == null || l.Select(i => i.EmployeeId).Distinct().Count() == l.Count).WithMessage("An employee cannot be listed twice.");
    }
}

public class TrainingEvaluationSaveValidator : AbstractValidator<TrainingEvaluationSaveDto>
{
    public TrainingEvaluationSaveValidator()
    {
        RuleFor(x => x).Must(x => x.Score.HasValue || !string.IsNullOrWhiteSpace(x.Feedback)).WithMessage("Provide a score or feedback.");
        RuleFor(x => x.Score).InclusiveBetween(0, 100).When(x => x.Score.HasValue).WithMessage("Score must be between 0 and 100.");
        RuleFor(x => x.Feedback).MaximumLength(1000);
    }
}

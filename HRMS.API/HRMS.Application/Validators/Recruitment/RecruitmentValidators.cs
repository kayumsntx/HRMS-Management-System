using FluentValidation;
using HRMS.Application.Common.Helpers;
using HRMS.Application.DTOs.Recruitment;
using HRMS.Domain.Enums;

namespace HRMS.Application.Validators.Recruitment;

public class ManpowerCreateValidator : AbstractValidator<ManpowerCreateDto>
{
    public ManpowerCreateValidator()
    {
        RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Please select a valid Department.");
        RuleFor(x => x.DesignationId).GreaterThan(0).WithMessage("Please select a valid Designation.");
        RuleFor(x => x.RequiredCount).InclusiveBetween(1, 1000).WithMessage("Required Count must be between 1 and 1000.");
        RuleFor(x => x.Justification).MaximumLength(500);
        RuleFor(x => x.RequiredDate).Must(d => d!.Value.Date >= DateTime.Now.Date).When(x => x.RequiredDate.HasValue).WithMessage("Required Date cannot be in the past.");
    }
}

public class VacancyCreateValidator : AbstractValidator<VacancyCreateDto>
{
    public VacancyCreateValidator()
    {
        RuleFor(x => x.ManpowerRequisitionId).GreaterThan(0).When(x => x.ManpowerRequisitionId.HasValue);
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.").MaximumLength(200);
        RuleFor(x => x.DepartmentId).GreaterThan(0).WithMessage("Please select a valid Department.");
        RuleFor(x => x.DesignationId).GreaterThan(0).WithMessage("Please select a valid Designation.");
        RuleFor(x => x.NumberOfVacancy).InclusiveBetween(1, 1000);
        RuleFor(x => x.JobDescription).MaximumLength(4000);
        RuleFor(x => x.Deadline).Must((dto, d) => d!.Value.Date >= (dto.PostedDate ?? DateTime.Now).Date)
            .When(x => x.Deadline.HasValue).WithMessage("Deadline cannot be before the Posted Date.");
    }
}

public class VacancyUpdateValidator : AbstractValidator<VacancyUpdateDto>
{
    public VacancyUpdateValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.").MaximumLength(200);
        RuleFor(x => x.NumberOfVacancy).InclusiveBetween(1, 1000);
        RuleFor(x => x.JobDescription).MaximumLength(4000);
    }
}

public class ApplicantCreateValidator : AbstractValidator<ApplicantCreateDto>
{
    public ApplicantCreateValidator()
    {
        RuleFor(x => x.JobVacancyId).GreaterThan(0).WithMessage("Please select a valid Vacancy.");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full Name is required.").MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Please provide a valid Email.").MaximumLength(150);
        RuleFor(x => x.MobileNumber).MaximumLength(20);
        RuleFor(x => x.ExpectedSalary).GreaterThanOrEqualTo(0).When(x => x.ExpectedSalary.HasValue);
        RuleFor(x => x.CvPath).MaximumLength(500);
        RuleFor(x => x.Source).MaximumLength(100);
    }
}

public class ApplicantUpdateValidator : AbstractValidator<ApplicantUpdateDto>
{
    public ApplicantUpdateValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full Name is required.").MaximumLength(150);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("Please provide a valid Email.").MaximumLength(150);
        RuleFor(x => x.MobileNumber).MaximumLength(20);
        RuleFor(x => x.ExpectedSalary).GreaterThanOrEqualTo(0).When(x => x.ExpectedSalary.HasValue);
        RuleFor(x => x.CvPath).MaximumLength(500);
        RuleFor(x => x.Source).MaximumLength(100);
    }
}

public class ApplicantStatusValidator : AbstractValidator<ApplicantStatusDto>
{
    public ApplicantStatusValidator()
    {
        RuleFor(x => x.Status).Must(v => EnumHelper.IsValid<ApplicantStatus>(v)).WithMessage("Invalid Status.");
    }
}

public class InterviewScheduleValidator : AbstractValidator<InterviewScheduleDto>
{
    public InterviewScheduleValidator()
    {
        RuleFor(x => x.ApplicantId).GreaterThan(0).WithMessage("Please select a valid Applicant.");
        RuleFor(x => x.InterviewDate).NotEmpty().Must(d => d > DateTime.Now.AddMinutes(-5)).WithMessage("Interview Date must be in the future.");
        RuleFor(x => x.Venue).MaximumLength(200);
    }
}

public class InterviewUpdateValidator : AbstractValidator<InterviewUpdateDto>
{
    public InterviewUpdateValidator()
    {
        RuleFor(x => x.InterviewDate).NotEmpty().Must(d => d > DateTime.Now.AddMinutes(-5)).WithMessage("Interview Date must be in the future.");
        RuleFor(x => x.Venue).MaximumLength(200);
    }
}

public class InterviewEvaluateValidator : AbstractValidator<InterviewEvaluateDto>
{
    public InterviewEvaluateValidator()
    {
        RuleFor(x => x.EvaluatorEmployeeId).GreaterThan(0).WithMessage("Please select a valid Evaluator.");
        RuleFor(x => x.Score).InclusiveBetween(0, 100).WithMessage("Score must be between 0 and 100.");
        RuleFor(x => x.Result).Must(v => EnumHelper.IsValid<InterviewResult>(v)).WithMessage("Result must be Pass, Fail or Hold.");
        RuleFor(x => x.Comments).MaximumLength(1000);
    }
}

public class OfferCreateValidator : AbstractValidator<OfferCreateDto>
{
    public OfferCreateValidator()
    {
        RuleFor(x => x.ApplicantId).GreaterThan(0).WithMessage("Please select a valid Applicant.");
        RuleFor(x => x.DesignationId).GreaterThan(0).When(x => x.DesignationId.HasValue);
        RuleFor(x => x.OfferedSalary).GreaterThan(0).LessThan(1_000_000_000m);
        RuleFor(x => x.JoiningDate).Must((dto, d) => d!.Value.Date >= (dto.OfferDate ?? DateTime.Now).Date)
            .When(x => x.JoiningDate.HasValue).WithMessage("Joining Date cannot be before the Offer Date.");
    }
}

public class OfferStatusValidator : AbstractValidator<OfferStatusDto>
{
    public OfferStatusValidator()
    {
        RuleFor(x => x.Status).Must(v => EnumHelper.IsValid<OfferStatus>(v)).WithMessage("Invalid Status. Allowed: Accepted, Rejected, Expired.");
    }
}

public class HireValidator : AbstractValidator<HireDto>
{
    public HireValidator()
    {
        RuleFor(x => x.CompanyId).GreaterThan(0).WithMessage("Please select a valid Company.");
        RuleFor(x => x.EmployeeStatusId).GreaterThan(0).WithMessage("Please select a valid Employee Status.");
        RuleFor(x => x.GradeId).GreaterThan(0).When(x => x.GradeId.HasValue);
        RuleFor(x => x.UnitId).GreaterThan(0).When(x => x.UnitId.HasValue);
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId.HasValue);
        RuleFor(x => x.ReportingManagerId).GreaterThan(0).When(x => x.ReportingManagerId.HasValue);
    }
}

namespace HRMS.Application.DTOs.Recruitment;

// ---- Manpower requisition ----
public class ManpowerCreateDto
{
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public int RequiredCount { get; set; }
    public string? Justification { get; set; }
    public DateTime? RequiredDate { get; set; }
}

public class ManpowerResponseDto
{
    public int Id { get; set; }
    public string RequisitionNo { get; set; } = default!;
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public int RequiredCount { get; set; }
    public int VacancyCount { get; set; }
    public string? Justification { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = default!;
    public int? RequestedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedDate { get; set; }
}

// ---- Vacancy ----
public class VacancyCreateDto
{
    public int? ManpowerRequisitionId { get; set; }
    public string Title { get; set; } = default!;
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public int NumberOfVacancy { get; set; }
    public string? JobDescription { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? Deadline { get; set; }
}

public class VacancyUpdateDto
{
    public string Title { get; set; } = default!;
    public int NumberOfVacancy { get; set; }
    public string? JobDescription { get; set; }
    public DateTime? Deadline { get; set; }
}

public class VacancyResponseDto
{
    public int Id { get; set; }
    public int? ManpowerRequisitionId { get; set; }
    public string Title { get; set; } = default!;
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public int NumberOfVacancy { get; set; }
    public string? JobDescription { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = default!;
    public int ApplicantCount { get; set; }
    public int HiredCount { get; set; }
}

// ---- Applicant ----
public class ApplicantCreateDto
{
    public int JobVacancyId { get; set; }
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? CvPath { get; set; }
    public string? Source { get; set; }
}

public class ApplicantUpdateDto
{
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? CvPath { get; set; }
    public string? Source { get; set; }
}

public class ApplicantStatusDto
{
    public string Status { get; set; } = default!;   // Shortlisted, Selected, Rejected
}

public class ApplicantResponseDto
{
    public int Id { get; set; }
    public int JobVacancyId { get; set; }
    public string? VacancyTitle { get; set; }
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? CvPath { get; set; }
    public string? Source { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Status { get; set; } = default!;
}

// ---- Interview ----
public class InterviewScheduleDto
{
    public int ApplicantId { get; set; }
    public DateTime InterviewDate { get; set; }
    public string? Venue { get; set; }
}

public class InterviewUpdateDto
{
    public DateTime InterviewDate { get; set; }
    public string? Venue { get; set; }
}

public class InterviewEvaluateDto
{
    public int EvaluatorEmployeeId { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
    public string Result { get; set; } = default!;   // Pass, Fail, Hold
}

public class InterviewEvaluationResponseDto
{
    public int Id { get; set; }
    public int InterviewId { get; set; }
    public int? EvaluatorEmployeeId { get; set; }
    public string? EvaluatorName { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
    public string Result { get; set; } = default!;
    public DateTime EvaluatedDate { get; set; }
}

public class InterviewResponseDto
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public string? ApplicantName { get; set; }
    public string? VacancyTitle { get; set; }
    public int InterviewRound { get; set; }
    public DateTime InterviewDate { get; set; }
    public string? Venue { get; set; }
    public string Status { get; set; } = default!;
    public List<InterviewEvaluationResponseDto> Evaluations { get; set; } = new();
}

// ---- Offer / Hire ----
public class OfferCreateDto
{
    public int ApplicantId { get; set; }
    public int? DesignationId { get; set; }          // default = the vacancy's designation
    public decimal OfferedSalary { get; set; }
    public DateTime? OfferDate { get; set; }
    public DateTime? JoiningDate { get; set; }
}

public class OfferStatusDto
{
    public string Status { get; set; } = default!;   // Accepted, Rejected, Expired
}

public class HireDto
{
    public int CompanyId { get; set; }
    public int EmployeeStatusId { get; set; }
    public int? GradeId { get; set; }
    public int? UnitId { get; set; }
    public int? SectionId { get; set; }
    public int? ReportingManagerId { get; set; }
    public DateTime? JoiningDate { get; set; }       // default = offer joining date or today
}

public class OfferResponseDto
{
    public int Id { get; set; }
    public int ApplicantId { get; set; }
    public string? ApplicantName { get; set; }
    public int DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public decimal OfferedSalary { get; set; }
    public DateTime OfferDate { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string Status { get; set; } = default!;
    public int? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
}

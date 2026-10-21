using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class ManpowerRequisition : BaseEntity
{
    public string RequisitionNo { get; set; } = default!;
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public int RequiredCount { get; set; }
    public string? Justification { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = "Pending";
    public int? RequestedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
}

public class JobVacancy : BaseEntity
{
    public int? ManpowerRequisitionId { get; set; }
    public string Title { get; set; } = default!;
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public int NumberOfVacancy { get; set; }
    public string? JobDescription { get; set; }
    public DateTime PostedDate { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = "Open";
}

public class Applicant : BaseEntity
{
    public int JobVacancyId { get; set; }
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? MobileNumber { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? CvPath { get; set; }
    public string? Source { get; set; }
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Applied";
}

public class Interview : BaseEntity
{
    public int ApplicantId { get; set; }
    public int InterviewRound { get; set; } = 1;
    public DateTime InterviewDate { get; set; }
    public string? Venue { get; set; }
    public string Status { get; set; } = "Scheduled";
}

public class InterviewEvaluation : BaseEntity
{
    public int InterviewId { get; set; }
    public int? EvaluatorEmployeeId { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
    public string Result { get; set; } = default!;
    public DateTime EvaluatedDate { get; set; } = DateTime.UtcNow;
}

public class JobOffer : BaseEntity
{
    public int ApplicantId { get; set; }
    public int DesignationId { get; set; }
    public decimal OfferedSalary { get; set; }
    public DateTime OfferDate { get; set; }
    public DateTime? JoiningDate { get; set; }
    public string Status { get; set; } = "Sent";
    public int? EmployeeId { get; set; }
}

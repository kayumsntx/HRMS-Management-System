using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class EmployeeSeparation : BaseEntity
{
    public int EmployeeId { get; set; }
    public string SeparationType { get; set; } = default!;
    public DateTime RequestDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class ExitInterview : BaseEntity
{
    public int SeparationId { get; set; }
    public DateTime InterviewDate { get; set; }
    public int? InterviewerEmployeeId { get; set; }
    public string? Feedback { get; set; }
    public bool? WouldRejoin { get; set; }
}

public class SeparationClearance : BaseEntity
{
    public int SeparationId { get; set; }
    public int DepartmentId { get; set; }
    public bool IsCleared { get; set; }
    public decimal DueAmount { get; set; }
    public int? ClearedBy { get; set; }
    public DateTime? ClearedDate { get; set; }
    public string? Remarks { get; set; }
}

public class FinalSettlement : BaseEntity
{
    public int SeparationId { get; set; }
    public decimal PayableSalary { get; set; }
    public decimal LeaveEncashment { get; set; }
    public decimal OtherPayable { get; set; }
    public decimal OtherDeduction { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetPayable { get; set; }
    public DateTime? SettlementDate { get; set; }
    public string Status { get; set; } = "Draft";
    public int? ApprovedBy { get; set; }
}

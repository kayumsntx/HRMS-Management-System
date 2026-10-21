using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class EmployeeCareerHistory : BaseEntity
{
    public int EmployeeId { get; set; }
    public string ChangeType { get; set; } = default!;
    public int? OldDesignationId { get; set; }
    public int? NewDesignationId { get; set; }
    public int? OldGradeId { get; set; }
    public int? NewGradeId { get; set; }
    public int? OldDepartmentId { get; set; }
    public int? NewDepartmentId { get; set; }
    public int? OldUnitId { get; set; }
    public int? NewUnitId { get; set; }
    public int? OldSectionId { get; set; }
    public int? NewSectionId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = "Pending";
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsApplied { get; set; }
    public DateTime? AppliedDate { get; set; }
}

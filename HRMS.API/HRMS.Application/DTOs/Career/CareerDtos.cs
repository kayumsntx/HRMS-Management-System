namespace HRMS.Application.DTOs.Career;

public class CareerChangeCreateDto
{
    public int EmployeeId { get; set; }
    public string ChangeType { get; set; } = default!;   // Promotion, Transfer, DepartmentChange, DesignationChange, GradeChange
    public int? NewDesignationId { get; set; }
    public int? NewGradeId { get; set; }
    public int? NewDepartmentId { get; set; }
    public int? NewUnitId { get; set; }
    public int? NewSectionId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Remarks { get; set; }
}

public class CareerChangeResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string ChangeType { get; set; } = default!;
    public int? OldDesignationId { get; set; }
    public string? OldDesignationName { get; set; }
    public int? NewDesignationId { get; set; }
    public string? NewDesignationName { get; set; }
    public int? OldGradeId { get; set; }
    public string? OldGradeName { get; set; }
    public int? NewGradeId { get; set; }
    public string? NewGradeName { get; set; }
    public int? OldDepartmentId { get; set; }
    public string? OldDepartmentName { get; set; }
    public int? NewDepartmentId { get; set; }
    public string? NewDepartmentName { get; set; }
    public int? OldUnitId { get; set; }
    public string? OldUnitName { get; set; }
    public int? NewUnitId { get; set; }
    public string? NewUnitName { get; set; }
    public int? OldSectionId { get; set; }
    public string? OldSectionName { get; set; }
    public int? NewSectionId { get; set; }
    public string? NewSectionName { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Remarks { get; set; }
    public string Status { get; set; } = default!;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsApplied { get; set; }
    public DateTime? AppliedDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

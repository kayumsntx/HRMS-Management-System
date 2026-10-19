namespace HRMS.Application.DTOs.ShiftAssignments;

public class ShiftAssignmentResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public int ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

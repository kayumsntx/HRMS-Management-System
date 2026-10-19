namespace HRMS.Application.DTOs.ShiftAssignments;

public class ShiftAssignmentCreateDto
{
    public int EmployeeId { get; set; }
    public int ShiftId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

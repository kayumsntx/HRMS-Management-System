namespace HRMS.Application.DTOs.Leave;

public class LeaveTypeUpdateDto
{
    public string LeaveTypeName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; }
}

namespace HRMS.Application.DTOs.Leave;

public class LeaveTypeCreateDto
{
    public string LeaveTypeName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsPaid { get; set; } = true;
}

namespace HRMS.Application.DTOs.Leave;

public class LeaveTypeResponseDto
{
    public int Id { get; set; }
    public string LeaveTypeName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

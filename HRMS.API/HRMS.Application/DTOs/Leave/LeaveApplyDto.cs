namespace HRMS.Application.DTOs.Leave;

public class LeaveApplyDto
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? Reason { get; set; }
}

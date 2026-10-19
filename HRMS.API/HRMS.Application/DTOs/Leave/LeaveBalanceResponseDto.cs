namespace HRMS.Application.DTOs.Leave;

public class LeaveBalanceResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public int LeaveYear { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
}

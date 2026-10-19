namespace HRMS.Application.DTOs.Leave;

public class LeaveBalanceCreateDto
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int LeaveYear { get; set; }
    public decimal OpeningBalance { get; set; }
}

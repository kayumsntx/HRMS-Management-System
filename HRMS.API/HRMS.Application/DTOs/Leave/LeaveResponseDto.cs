namespace HRMS.Application.DTOs.Leave;

public class LeaveResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal NumberOfDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = default!;
    public DateTime AppliedDate { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
}

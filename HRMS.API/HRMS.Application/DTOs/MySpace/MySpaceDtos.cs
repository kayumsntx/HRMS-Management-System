namespace HRMS.Application.DTOs.MySpace;

public class MyLeaveApplyDto
{
    public int LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? Reason { get; set; }
}

public class MyPayslipSummaryDto
{
    public int PayrollId { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public string Status { get; set; } = default!;
    public decimal GrossSalary { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
}

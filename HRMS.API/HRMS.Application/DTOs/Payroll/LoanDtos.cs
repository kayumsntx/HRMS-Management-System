namespace HRMS.Application.DTOs.Payroll;

public class LoanCreateDto
{
    public int EmployeeId { get; set; }
    public decimal LoanAmount { get; set; }
    public int TotalInstallments { get; set; }
    public DateTime StartDate { get; set; }       // first deduction month (any day of that month)
    public string? Purpose { get; set; }
}

public class MyLoanCreateDto
{
    public decimal LoanAmount { get; set; }
    public int TotalInstallments { get; set; }
    public DateTime StartDate { get; set; }
    public string? Purpose { get; set; }
}

public class LoanResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int TotalInstallments { get; set; }
    public int PaidInstallments { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = default!;
    public string? Purpose { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
}

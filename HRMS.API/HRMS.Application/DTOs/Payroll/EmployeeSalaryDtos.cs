namespace HRMS.Application.DTOs.Payroll;

public class EmployeeSalaryCreateDto
{
    public int EmployeeId { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal? OvertimeRatePerHour { get; set; }   // optional; default = Basic / 208 x 2
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class EmployeeSalaryResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal AllowanceAmount { get; set; }        // Gross - Basic
    public decimal GrossSalary { get; set; }
    public decimal? OvertimeRatePerHour { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedDate { get; set; }
}

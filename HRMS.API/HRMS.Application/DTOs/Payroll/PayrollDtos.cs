namespace HRMS.Application.DTOs.Payroll;

public class PayrollProcessDto
{
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
}

public class PayrollDetailAdjustDto
{
    public decimal BonusAmount { get; set; }
    public decimal TotalDeduction { get; set; }
}

public class PayrollResponseDto
{
    public int Id { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public DateTime ProcessDate { get; set; }
    public string Status { get; set; } = default!;
    public int? ProcessedBy { get; set; }
    public int TotalEmployees { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNet { get; set; }
}

public class PayrollProcessResultDto
{
    public PayrollResponseDto Payroll { get; set; } = default!;
    public int EmployeesProcessed { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class PayrollDetailResponseDto
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DesignationName { get; set; }
    public string? DepartmentName { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
}

public class PayslipDto
{
    public int PayrollId { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public string Status { get; set; } = default!;

    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string EmployeeName { get; set; } = default!;
    public string? DesignationName { get; set; }
    public string? DepartmentName { get; set; }
    public string? CompanyName { get; set; }
    public DateTime DateOfJoining { get; set; }

    public decimal BasicSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal GrossSalary { get; set; }          // total earnings
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }

    public int DaysInMonth { get; set; }
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int HalfDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public int WeekendDays { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
}

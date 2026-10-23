namespace HRMS.Application.DTOs.Payroll;

public class SalaryComponentCreateDto
{
    public string ComponentName { get; set; } = default!;
    public string ComponentType { get; set; } = default!;   // Allowance / Deduction
}

public class SalaryComponentUpdateDto : SalaryComponentCreateDto
{
    public bool IsActive { get; set; } = true;
}

public class SalaryComponentResponseDto
{
    public int Id { get; set; }
    public string ComponentName { get; set; } = default!;
    public string ComponentType { get; set; } = default!;
    public bool IsActive { get; set; }
}

public class SalaryBreakdownItemDto
{
    public int SalaryComponentId { get; set; }
    public decimal Amount { get; set; }
}

public class SalaryBreakdownSaveDto
{
    public List<SalaryBreakdownItemDto> Items { get; set; } = new();
}

public class SalaryBreakdownLineDto
{
    public int SalaryComponentId { get; set; }
    public string? ComponentName { get; set; }
    public string? ComponentType { get; set; }
    public decimal Amount { get; set; }
}

public class SalaryBreakdownDto
{
    public int EmployeeSalaryId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal AllowanceTotal { get; set; }
    public decimal DeductionTotal { get; set; }
    public List<SalaryBreakdownLineDto> Lines { get; set; } = new();
}

namespace HRMS.Application.DTOs.Leave;

public class LeavePolicyCreateDto
{
    public int LeaveTypeId { get; set; }
    public decimal YearlyDays { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class LeavePolicyUpdateDto : LeavePolicyCreateDto
{
    public bool IsActive { get; set; } = true;
}

public class LeavePolicyResponseDto
{
    public int Id { get; set; }
    public int LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public decimal YearlyDays { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public class LeaveBalanceGenerateDto
{
    public int Year { get; set; }
    public int? EmployeeId { get; set; }   // null = all active employees
}

public class LeaveBalanceGenerateResultDto
{
    public int Year { get; set; }
    public int EmployeesProcessed { get; set; }
    public int Created { get; set; }
    public int SkippedExisting { get; set; }
    public int PoliciesApplied { get; set; }
}

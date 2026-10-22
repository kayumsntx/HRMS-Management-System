using HRMS.Application.DTOs.Common;

namespace HRMS.Application.DTOs.Payroll;

public class BonusCreateDto : IHasEmployeeId
{
    public int EmployeeId { get; set; }
    public string BonusType { get; set; } = default!;
    public decimal Amount { get; set; }
    public int BonusMonth { get; set; }
    public int BonusYear { get; set; }
    public string? Remarks { get; set; }
}

public class BonusUpdateDto
{
    public string BonusType { get; set; } = default!;
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
}

public class BonusResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string BonusType { get; set; } = default!;
    public decimal Amount { get; set; }
    public int BonusMonth { get; set; }
    public int BonusYear { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
}

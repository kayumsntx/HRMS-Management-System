using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class LeavePolicy : BaseEntity
{
    public int LeaveTypeId { get; set; }
    public decimal YearlyDays { get; set; }
    public decimal MaxCarryForwardDays { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class Bonus : BaseEntity, IEmployeeOwned
{
    public int EmployeeId { get; set; }
    public string BonusType { get; set; } = default!;
    public decimal Amount { get; set; }
    public int BonusMonth { get; set; }
    public int BonusYear { get; set; }
    public string? Remarks { get; set; }
}

public class Loan : BaseEntity, IEmployeeOwned
{
    public int EmployeeId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int TotalInstallments { get; set; }
    public int PaidInstallments { get; set; }
    public DateTime StartDate { get; set; }               // first day of the first deduction month
    public string Status { get; set; } = "Pending";
    public string? Purpose { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
}

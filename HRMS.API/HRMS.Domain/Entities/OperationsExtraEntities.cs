using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class ShortLeave : BaseEntity, IEmployeeOwned
{
    public int EmployeeId { get; set; }
    public DateTime LeaveDate { get; set; }
    public TimeSpan FromTime { get; set; }
    public TimeSpan ToTime { get; set; }
    public decimal TotalHours { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
}

// A row with IsActive = true means the month's attendance is approved (locked).
public class AttendanceMonthLock : BaseEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? Remarks { get; set; }
}

public class BiometricSyncHistory : BaseEntity
{
    public int BiometricDeviceId { get; set; }
    public string SyncType { get; set; } = "PunchImport";
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public int TotalRecords { get; set; }
    public int NewRecords { get; set; }
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
}

public class SalaryComponent : BaseEntity
{
    public string ComponentName { get; set; } = default!;
    public string ComponentType { get; set; } = default!;   // Allowance / Deduction
}

public class EmployeeSalaryComponent : BaseEntity
{
    public int EmployeeSalaryId { get; set; }
    public int SalaryComponentId { get; set; }
    public decimal Amount { get; set; }
}

namespace HRMS.Application.DTOs.Attendance;

public class ShortLeaveCreateDto
{
    public int EmployeeId { get; set; }
    public DateTime LeaveDate { get; set; }
    public TimeSpan FromTime { get; set; }
    public TimeSpan ToTime { get; set; }
    public string? Reason { get; set; }
}

public class MyShortLeaveCreateDto
{
    public DateTime LeaveDate { get; set; }
    public TimeSpan FromTime { get; set; }
    public TimeSpan ToTime { get; set; }
    public string? Reason { get; set; }
}

public class ShortLeaveResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime LeaveDate { get; set; }
    public TimeSpan FromTime { get; set; }
    public TimeSpan ToTime { get; set; }
    public decimal TotalHours { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = default!;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
}

public class AttendanceLockDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string? Remarks { get; set; }
}

public class AttendanceLockResponseDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IsLocked { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? Remarks { get; set; }
}

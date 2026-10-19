namespace HRMS.Application.DTOs.Attendance;

public class AttendanceProcessDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? EmployeeId { get; set; }   // null = all active employees
}

public class AttendanceProcessResultDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int EmployeesProcessed { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int SkippedNoShift { get; set; }     // employee-days without a shift assignment
    public int SkippedManual { get; set; }      // records protected because they were manually corrected
}

public class AttendanceCorrectionDto
{
    public DateTime? FirstInTime { get; set; }
    public DateTime? LastOutTime { get; set; }
    public string? DayStatus { get; set; }      // optional override: Present, Absent, Late, Leave, Holiday, Weekend, HalfDay
    public string Remarks { get; set; } = default!;
}

public class AttendanceFilterDto
{
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? DayStatus { get; set; }
}

public class AttendanceResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime WorkingDate { get; set; }
    public int ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public DateTime? FirstInTime { get; set; }
    public DateTime? LastOutTime { get; set; }
    public decimal? WorkingHours { get; set; }
    public decimal? LateHours { get; set; }
    public decimal? EarlyOutHours { get; set; }
    public decimal? OTHours { get; set; }
    public string DayStatus { get; set; } = default!;
    public decimal? TotalShortLeave { get; set; }
    public decimal? ActualShortLeave { get; set; }
    public string? Remarks { get; set; }
}

public class AttendanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int DaysInMonth { get; set; }
    public int ProcessedDays { get; set; }
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int HalfDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public int WeekendDays { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal TotalLateHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
}

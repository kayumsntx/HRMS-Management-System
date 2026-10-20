namespace HRMS.Application.DTOs.Reports;

public abstract class ReportFilterBase
{
    public string? Format { get; set; }   // json (default) or csv
}

// ---------- Employee report ----------
public class EmployeeReportFilterDto : ReportFilterBase
{
    public int? CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? EmployeeStatusId { get; set; }
    public bool? IsActive { get; set; }          // null = all employees
    public DateTime? JoinedFrom { get; set; }
    public DateTime? JoinedTo { get; set; }
}

public class EmployeeReportRowDto
{
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Gender { get; set; }
    public string? CompanyName { get; set; }
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public string? EmployeeStatus { get; set; }
    public DateTime DateOfJoining { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}

// ---------- Monthly attendance ----------
public class MonthlyAttendanceFilterDto : ReportFilterBase
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int? DepartmentId { get; set; }
}

public class MonthlyAttendanceRowDto
{
    public string EmployeeCode { get; set; } = default!;
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public int DaysInMonth { get; set; }
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int HalfDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public int WeekendDays { get; set; }
    public int HolidayDays { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public decimal TotalLateHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
}

// ---------- Daily attendance ----------
public class DailyAttendanceFilterDto : ReportFilterBase
{
    public DateTime Date { get; set; }
    public int? DepartmentId { get; set; }
    public string? DayStatus { get; set; }
}

public class DailyAttendanceRowDto
{
    public string EmployeeCode { get; set; } = default!;
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? ShiftName { get; set; }
    public DateTime? FirstInTime { get; set; }
    public DateTime? LastOutTime { get; set; }
    public decimal? WorkingHours { get; set; }
    public decimal? LateHours { get; set; }
    public string DayStatus { get; set; } = default!;
    public string? Remarks { get; set; }
}

// ---------- Late report ----------
public class LateReportFilterDto : ReportFilterBase
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? DepartmentId { get; set; }
}

public class LateReportRowDto
{
    public DateTime Date { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? ShiftName { get; set; }
    public DateTime? FirstInTime { get; set; }
    public decimal? LateHours { get; set; }
}

// ---------- Overtime report ----------
public class OvertimeReportFilterDto : ReportFilterBase
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int? DepartmentId { get; set; }
}

public class OvertimeReportRowDto
{
    public string EmployeeCode { get; set; } = default!;
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public int OvertimeDays { get; set; }
    public decimal TotalOvertimeHours { get; set; }
}

// ---------- Leave report ----------
public class LeaveReportFilterDto : ReportFilterBase
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? Status { get; set; }
    public int? LeaveTypeId { get; set; }
    public int? DepartmentId { get; set; }
}

public class LeaveReportRowDto
{
    public string EmployeeCode { get; set; } = default!;
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? LeaveType { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal NumberOfDays { get; set; }
    public string Status { get; set; } = default!;
    public string? Reason { get; set; }
}

// ---------- Payroll report ----------
public class PayrollReportFilterDto : ReportFilterBase
{
    public int PayrollId { get; set; }
}

public class PayrollReportRowDto
{
    public string EmployeeCode { get; set; } = default!;
    public string EmployeeName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowance { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetSalary { get; set; }
}

public class PayrollDepartmentSummaryDto
{
    public string DepartmentName { get; set; } = default!;
    public int Employees { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNet { get; set; }
}

public class PayrollReportDto
{
    public int PayrollId { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public string Status { get; set; } = default!;
    public int TotalEmployees { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal TotalNet { get; set; }
    public List<PayrollDepartmentSummaryDto> DepartmentSummary { get; set; } = new();
    public List<PayrollReportRowDto> Rows { get; set; } = new();
}

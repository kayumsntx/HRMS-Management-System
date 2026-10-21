namespace HRMS.Application.DTOs.Dashboard;

public class DashboardPersonDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? Detail { get; set; }
}

public class DepartmentCountDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = default!;
    public int Count { get; set; }
}

public class DashboardEmployeeSummaryDto
{
    public int TotalActive { get; set; }
    public int Male { get; set; }
    public int Female { get; set; }
    public int Other { get; set; }
    public int JoinedThisMonth { get; set; }
    public List<DepartmentCountDto> ByDepartment { get; set; } = new();
}

public class DashboardSummaryDto
{
    public DateTime Date { get; set; }
    public bool AttendanceProcessed { get; set; }   // false = no attendance records exist for this date yet

    public DashboardEmployeeSummaryDto EmployeeSummary { get; set; } = new();

    public int PresentToday { get; set; }           // Present + Late + HalfDay
    public int AbsentToday { get; set; }
    public int LateToday { get; set; }
    public int OnLeaveToday { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int PendingRequisitions { get; set; }
    public int PendingCareerChanges { get; set; }
    public int PendingSeparations { get; set; }
    public int OpenVacancies { get; set; }

    public List<DashboardPersonDto> LateEmployees { get; set; } = new();
    public List<DashboardPersonDto> AbsentEmployees { get; set; } = new();
    public List<DashboardPersonDto> OnLeaveEmployees { get; set; } = new();
    public List<DashboardPersonDto> NewJoiners { get; set; } = new();       // last 30 days
    public List<DashboardPersonDto> Birthdays { get; set; } = new();        // today + next 7 days
}

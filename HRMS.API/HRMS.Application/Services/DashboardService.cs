using HRMS.Application.DTOs.Dashboard;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class DashboardService : IDashboardService
{
    private const int NewJoinerDays = 30;
    private const int BirthdayDays = 7;
    private const int ListLimit = 20;

    private static readonly string StatusPresent = AttendanceDayStatus.Present.ToString();
    private static readonly string StatusLate = AttendanceDayStatus.Late.ToString();
    private static readonly string StatusHalfDay = AttendanceDayStatus.HalfDay.ToString();
    private static readonly string StatusAbsent = AttendanceDayStatus.Absent.ToString();

    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime? date)
    {
        var day = (date ?? DateTime.Now).Date;
        var nextDay = day.AddDays(1);

        var employees = await _unitOfWork.Repository<Employee>().FindAsync(e => e.IsActive);
        var employeeById = employees.ToDictionary(e => e.Id);
        var activeIds = employeeById.Keys.ToHashSet();

        var departments = (await _unitOfWork.Repository<Department>().GetAllAsync())
            .ToDictionary(d => d.Id, d => d.DepartmentName);

        DashboardPersonDto Person(Employee e, string? detail) => new()
        {
            EmployeeId = e.Id,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            DepartmentName = departments.GetValueOrDefault(e.DepartmentId),
            Detail = detail
        };

        // ---------- Employee summary ----------
        var summary = new DashboardEmployeeSummaryDto
        {
            TotalActive = employees.Count,
            Male = employees.Count(e => string.Equals(e.Gender, "Male", StringComparison.OrdinalIgnoreCase)),
            Female = employees.Count(e => string.Equals(e.Gender, "Female", StringComparison.OrdinalIgnoreCase)),
            JoinedThisMonth = employees.Count(e => e.DateOfJoining.Year == day.Year && e.DateOfJoining.Month == day.Month && e.DateOfJoining.Date <= day)
        };
        summary.Other = summary.TotalActive - summary.Male - summary.Female;

        summary.ByDepartment = employees
            .GroupBy(e => e.DepartmentId)
            .Select(g => new DepartmentCountDto
            {
                DepartmentId = g.Key,
                DepartmentName = departments.GetValueOrDefault(g.Key) ?? "-",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // ---------- Attendance of the day ----------
        var records = (await _unitOfWork.Repository<EmployeeAttendance>()
                .FindAsync(a => a.WorkingDate >= day && a.WorkingDate < nextDay))
            .Where(a => activeIds.Contains(a.EmployeeId))
            .ToList();

        var lateRecords = records.Where(r => r.DayStatus == StatusLate).ToList();
        var absentRecords = records.Where(r => r.DayStatus == StatusAbsent).ToList();

        // ---------- Leave ----------
        var approvedText = LeaveStatus.Approved.ToString();
        var pendingText = LeaveStatus.Pending.ToString();

        var leaveTypes = (await _unitOfWork.Repository<LeaveType>().GetAllAsync())
            .ToDictionary(l => l.Id, l => l.LeaveTypeName);

        var leavesToday = (await _unitOfWork.Repository<EmployeeLeave>()
                .FindAsync(l => l.Status == approvedText && l.FromDate <= day && l.ToDate >= day))
            .Where(l => activeIds.Contains(l.EmployeeId))
            .GroupBy(l => l.EmployeeId)
            .Select(g => g.First())
            .ToList();

        var pendingLeaves = await _unitOfWork.Repository<EmployeeLeave>().FindAsync(l => l.Status == pendingText);
        var pendingRequisitions = await _unitOfWork.Repository<Requisition>().FindAsync(r => r.Status == pendingText);
        var pendingCareerChanges = await _unitOfWork.Repository<EmployeeCareerHistory>().FindAsync(c => c.Status == pendingText);
        var pendingSeparations = await _unitOfWork.Repository<EmployeeSeparation>().FindAsync(s => s.Status == pendingText);
        var openVacancies = await _unitOfWork.Repository<JobVacancy>().FindAsync(v => v.Status == "Open");

        // ---------- New joiners ----------
        var joinFrom = day.AddDays(-NewJoinerDays);
        var newJoiners = employees
            .Where(e => e.DateOfJoining.Date >= joinFrom && e.DateOfJoining.Date <= day)
            .OrderByDescending(e => e.DateOfJoining)
            .Take(ListLimit)
            .Select(e => Person(e, "Joined " + e.DateOfJoining.ToString("dd MMM yyyy")))
            .ToList();

        // ---------- Birthdays ----------
        var birthdays = employees
            .Where(e => e.DateOfBirth.HasValue)
            .Select(e => new { Employee = e, Next = NextBirthday(e.DateOfBirth!.Value, day) })
            .Where(x => (x.Next - day).Days <= BirthdayDays)
            .OrderBy(x => x.Next)
            .ThenBy(x => x.Employee.FullName)
            .Take(ListLimit)
            .Select(x => Person(
                x.Employee,
                (x.Next == day ? "Today" : x.Next.ToString("dd MMM")) + $" (turns {x.Next.Year - x.Employee.DateOfBirth!.Value.Year})"))
            .ToList();

        return new DashboardSummaryDto
        {
            Date = day,
            AttendanceProcessed = records.Count > 0,
            EmployeeSummary = summary,

            PresentToday = records.Count(r => r.DayStatus == StatusPresent || r.DayStatus == StatusLate || r.DayStatus == StatusHalfDay),
            AbsentToday = absentRecords.Count,
            LateToday = lateRecords.Count,
            OnLeaveToday = leavesToday.Count,
            PendingLeaveRequests = pendingLeaves.Count,
            PendingRequisitions = pendingRequisitions.Count,
            PendingCareerChanges = pendingCareerChanges.Count,
            PendingSeparations = pendingSeparations.Count,
            OpenVacancies = openVacancies.Count,

            LateEmployees = lateRecords
                .OrderByDescending(r => r.LateHours)
                .Take(ListLimit)
                .Select(r => Person(employeeById[r.EmployeeId], $"In {r.FirstInTime:HH:mm}, {r.LateHours:0.##}h late"))
                .ToList(),

            AbsentEmployees = absentRecords
                .Take(ListLimit)
                .Select(r => Person(employeeById[r.EmployeeId], null))
                .ToList(),

            OnLeaveEmployees = leavesToday
                .Take(ListLimit)
                .Select(l => Person(employeeById[l.EmployeeId],
                    $"{leaveTypes.GetValueOrDefault(l.LeaveTypeId)} until {l.ToDate:dd MMM}"))
                .ToList(),

            NewJoiners = newJoiners,
            Birthdays = birthdays
        };
    }

    // Next occurrence (today or later) of a birthday. 29 Feb becomes 28 Feb in non-leap years.
    private static DateTime NextBirthday(DateTime dateOfBirth, DateTime today)
    {
        DateTime Build(int year)
        {
            var d = dateOfBirth.Day;
            if (dateOfBirth.Month == 2 && d == 29 && !DateTime.IsLeapYear(year))
                d = 28;
            return new DateTime(year, dateOfBirth.Month, d);
        }

        var next = Build(today.Year);
        if (next < today)
            next = Build(today.Year + 1);

        return next;
    }
}

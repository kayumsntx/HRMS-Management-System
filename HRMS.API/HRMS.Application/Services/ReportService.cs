using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Reports;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class ReportService : IReportService
{
    private static readonly string StatusPresent = AttendanceDayStatus.Present.ToString();
    private static readonly string StatusAbsent = AttendanceDayStatus.Absent.ToString();
    private static readonly string StatusLate = AttendanceDayStatus.Late.ToString();
    private static readonly string StatusLeave = AttendanceDayStatus.Leave.ToString();
    private static readonly string StatusWeekend = AttendanceDayStatus.Weekend.ToString();
    private static readonly string StatusHoliday = AttendanceDayStatus.Holiday.ToString();
    private static readonly string StatusHalfDay = AttendanceDayStatus.HalfDay.ToString();

    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private sealed class Lookups
    {
        public Dictionary<int, Employee> Employees { get; set; } = new();
        public Dictionary<int, string> Departments { get; set; } = new();
        public Dictionary<int, string> Designations { get; set; } = new();
        public Dictionary<int, string> Companies { get; set; } = new();
        public Dictionary<int, string> EmployeeStatuses { get; set; } = new();
        public Dictionary<int, string> Shifts { get; set; } = new();
        public Dictionary<int, string> LeaveTypes { get; set; } = new();
    }

    private async Task<Lookups> LoadLookupsAsync() => new Lookups
    {
        Employees = (await _unitOfWork.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id),
        Departments = (await _unitOfWork.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName),
        Designations = (await _unitOfWork.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName),
        Companies = (await _unitOfWork.Repository<Company>().GetAllAsync()).ToDictionary(c => c.Id, c => c.CompanyName),
        EmployeeStatuses = (await _unitOfWork.Repository<EmployeeStatus>().GetAllAsync()).ToDictionary(s => s.Id, s => s.StatusName),
        Shifts = (await _unitOfWork.Repository<Shift>().GetAllAsync()).ToDictionary(s => s.Id, s => s.ShiftName),
        LeaveTypes = (await _unitOfWork.Repository<LeaveType>().GetAllAsync()).ToDictionary(l => l.Id, l => l.LeaveTypeName)
    };

    private static bool InDepartment(Lookups lookups, int employeeId, int? departmentId)
        => lookups.Employees.TryGetValue(employeeId, out var e) && (!departmentId.HasValue || e.DepartmentId == departmentId.Value);

    private static int CodeSortLength(string code) => code.Length;

    // =====================================================================
    // EMPLOYEE REPORT
    // =====================================================================
    public async Task<List<EmployeeReportRowDto>> GetEmployeeReportAsync(EmployeeReportFilterDto filter)
    {
        var companyId = filter.CompanyId;
        var departmentId = filter.DepartmentId;
        var designationId = filter.DesignationId;
        var statusId = filter.EmployeeStatusId;
        var isActive = filter.IsActive;
        var joinedFrom = filter.JoinedFrom?.Date;
        var joinedTo = filter.JoinedTo?.Date;

        var employees = await _unitOfWork.Repository<Employee>().FindAsync(e =>
            (!companyId.HasValue || e.CompanyId == companyId.Value) &&
            (!departmentId.HasValue || e.DepartmentId == departmentId.Value) &&
            (!designationId.HasValue || e.DesignationId == designationId.Value) &&
            (!statusId.HasValue || e.EmployeeStatusId == statusId.Value) &&
            (!isActive.HasValue || e.IsActive == isActive.Value) &&
            (!joinedFrom.HasValue || e.DateOfJoining >= joinedFrom.Value) &&
            (!joinedTo.HasValue || e.DateOfJoining <= joinedTo.Value));

        var lookups = await LoadLookupsAsync();

        return employees
            .OrderBy(e => CodeSortLength(e.EmployeeCode)).ThenBy(e => e.EmployeeCode)
            .Select(e => new EmployeeReportRowDto
            {
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Gender = e.Gender,
                CompanyName = lookups.Companies.GetValueOrDefault(e.CompanyId),
                DepartmentName = lookups.Departments.GetValueOrDefault(e.DepartmentId),
                DesignationName = lookups.Designations.GetValueOrDefault(e.DesignationId),
                EmployeeStatus = lookups.EmployeeStatuses.GetValueOrDefault(e.EmployeeStatusId),
                DateOfJoining = e.DateOfJoining,
                MobileNumber = e.MobileNumber,
                Email = e.Email,
                IsActive = e.IsActive
            })
            .ToList();
    }

    // =====================================================================
    // MONTHLY ATTENDANCE (one row per employee)
    // =====================================================================
    public async Task<List<MonthlyAttendanceRowDto>> GetMonthlyAttendanceAsync(MonthlyAttendanceFilterDto filter)
    {
        var start = new DateTime(filter.Year, filter.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var daysInMonth = DateTime.DaysInMonth(filter.Year, filter.Month);

        var records = await _unitOfWork.Repository<EmployeeAttendance>()
            .FindAsync(a => a.WorkingDate >= start && a.WorkingDate <= end);

        var lookups = await LoadLookupsAsync();

        return records
            .GroupBy(a => a.EmployeeId)
            .Where(g => InDepartment(lookups, g.Key, filter.DepartmentId))
            .Select(g =>
            {
                var employee = lookups.Employees[g.Key];
                return new MonthlyAttendanceRowDto
                {
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    DepartmentName = lookups.Departments.GetValueOrDefault(employee.DepartmentId),
                    DaysInMonth = daysInMonth,
                    PresentDays = g.Count(r => r.DayStatus == StatusPresent),
                    LateDays = g.Count(r => r.DayStatus == StatusLate),
                    HalfDays = g.Count(r => r.DayStatus == StatusHalfDay),
                    AbsentDays = g.Count(r => r.DayStatus == StatusAbsent),
                    LeaveDays = g.Count(r => r.DayStatus == StatusLeave),
                    WeekendDays = g.Count(r => r.DayStatus == StatusWeekend),
                    HolidayDays = g.Count(r => r.DayStatus == StatusHoliday),
                    TotalWorkingHours = g.Sum(r => r.WorkingHours ?? 0m),
                    TotalLateHours = g.Sum(r => r.LateHours ?? 0m),
                    TotalOvertimeHours = g.Sum(r => r.OTHours ?? 0m)
                };
            })
            .OrderBy(r => CodeSortLength(r.EmployeeCode)).ThenBy(r => r.EmployeeCode)
            .ToList();
    }

    // =====================================================================
    // DAILY ATTENDANCE
    // =====================================================================
    public async Task<List<DailyAttendanceRowDto>> GetDailyAttendanceAsync(DailyAttendanceFilterDto filter)
    {
        var day = filter.Date.Date;
        var nextDay = day.AddDays(1);
        var statusText = string.IsNullOrWhiteSpace(filter.DayStatus) ? null : ParseDayStatus(filter.DayStatus);

        var records = await _unitOfWork.Repository<EmployeeAttendance>().FindAsync(a =>
            a.WorkingDate >= day && a.WorkingDate < nextDay && (statusText == null || a.DayStatus == statusText));

        var lookups = await LoadLookupsAsync();

        return records
            .Where(r => InDepartment(lookups, r.EmployeeId, filter.DepartmentId))
            .Select(r =>
            {
                var employee = lookups.Employees[r.EmployeeId];
                return new DailyAttendanceRowDto
                {
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    DepartmentName = lookups.Departments.GetValueOrDefault(employee.DepartmentId),
                    ShiftName = lookups.Shifts.GetValueOrDefault(r.ShiftId),
                    FirstInTime = r.FirstInTime,
                    LastOutTime = r.LastOutTime,
                    WorkingHours = r.WorkingHours,
                    LateHours = r.LateHours,
                    DayStatus = r.DayStatus,
                    Remarks = r.Remarks
                };
            })
            .OrderBy(r => CodeSortLength(r.EmployeeCode)).ThenBy(r => r.EmployeeCode)
            .ToList();
    }

    // =====================================================================
    // LATE REPORT
    // =====================================================================
    public async Task<List<LateReportRowDto>> GetLateReportAsync(LateReportFilterDto filter)
    {
        var from = filter.FromDate.Date;
        var to = filter.ToDate.Date;

        var records = await _unitOfWork.Repository<EmployeeAttendance>().FindAsync(a =>
            a.WorkingDate >= from && a.WorkingDate <= to && a.DayStatus == StatusLate);

        var lookups = await LoadLookupsAsync();

        return records
            .Where(r => InDepartment(lookups, r.EmployeeId, filter.DepartmentId))
            .Select(r =>
            {
                var employee = lookups.Employees[r.EmployeeId];
                return new LateReportRowDto
                {
                    Date = r.WorkingDate.Date,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    DepartmentName = lookups.Departments.GetValueOrDefault(employee.DepartmentId),
                    ShiftName = lookups.Shifts.GetValueOrDefault(r.ShiftId),
                    FirstInTime = r.FirstInTime,
                    LateHours = r.LateHours
                };
            })
            .OrderBy(r => r.Date).ThenBy(r => CodeSortLength(r.EmployeeCode)).ThenBy(r => r.EmployeeCode)
            .ToList();
    }

    // =====================================================================
    // OVERTIME REPORT (totals per employee)
    // =====================================================================
    public async Task<List<OvertimeReportRowDto>> GetOvertimeReportAsync(OvertimeReportFilterDto filter)
    {
        var from = filter.FromDate.Date;
        var to = filter.ToDate.Date;

        var records = await _unitOfWork.Repository<EmployeeAttendance>().FindAsync(a =>
            a.WorkingDate >= from && a.WorkingDate <= to && a.OTHours > 0);

        var lookups = await LoadLookupsAsync();

        return records
            .GroupBy(r => r.EmployeeId)
            .Where(g => InDepartment(lookups, g.Key, filter.DepartmentId))
            .Select(g =>
            {
                var employee = lookups.Employees[g.Key];
                return new OvertimeReportRowDto
                {
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    DepartmentName = lookups.Departments.GetValueOrDefault(employee.DepartmentId),
                    OvertimeDays = g.Count(),
                    TotalOvertimeHours = g.Sum(r => r.OTHours ?? 0m)
                };
            })
            .OrderByDescending(r => r.TotalOvertimeHours)
            .ToList();
    }

    // =====================================================================
    // LEAVE REPORT (leaves overlapping the date range)
    // =====================================================================
    public async Task<List<LeaveReportRowDto>> GetLeaveReportAsync(LeaveReportFilterDto filter)
    {
        var from = filter.FromDate.Date;
        var to = filter.ToDate.Date;
        var leaveTypeId = filter.LeaveTypeId;
        var statusText = string.IsNullOrWhiteSpace(filter.Status) ? null : ParseLeaveStatus(filter.Status);

        var leaves = await _unitOfWork.Repository<EmployeeLeave>().FindAsync(l =>
            l.FromDate <= to && l.ToDate >= from &&
            (statusText == null || l.Status == statusText) &&
            (!leaveTypeId.HasValue || l.LeaveTypeId == leaveTypeId.Value));

        var lookups = await LoadLookupsAsync();

        return leaves
            .Where(l => InDepartment(lookups, l.EmployeeId, filter.DepartmentId))
            .Select(l =>
            {
                var employee = lookups.Employees[l.EmployeeId];
                return new LeaveReportRowDto
                {
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    DepartmentName = lookups.Departments.GetValueOrDefault(employee.DepartmentId),
                    LeaveType = lookups.LeaveTypes.GetValueOrDefault(l.LeaveTypeId),
                    FromDate = l.FromDate.Date,
                    ToDate = l.ToDate.Date,
                    NumberOfDays = l.NumberOfDays,
                    Status = l.Status,
                    Reason = l.Reason
                };
            })
            .OrderBy(r => r.FromDate).ThenBy(r => CodeSortLength(r.EmployeeCode)).ThenBy(r => r.EmployeeCode)
            .ToList();
    }

    // =====================================================================
    // PAYROLL REPORT
    // =====================================================================
    public async Task<PayrollReportDto> GetPayrollReportAsync(PayrollReportFilterDto filter)
    {
        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(filter.PayrollId)
            ?? throw new NotFoundException(nameof(Payroll), filter.PayrollId);

        var details = await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.PayrollId == filter.PayrollId);
        var lookups = await LoadLookupsAsync();

        var rows = details
            .Where(d => lookups.Employees.ContainsKey(d.EmployeeId))
            .Select(d =>
            {
                var employee = lookups.Employees[d.EmployeeId];
                return new PayrollReportRowDto
                {
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = employee.FullName,
                    DepartmentName = lookups.Departments.GetValueOrDefault(employee.DepartmentId),
                    DesignationName = lookups.Designations.GetValueOrDefault(employee.DesignationId),
                    BasicSalary = d.BasicSalary,
                    TotalAllowance = d.TotalAllowance,
                    OvertimeAmount = d.OvertimeAmount,
                    BonusAmount = d.BonusAmount,
                    GrossSalary = d.GrossSalary,
                    TotalDeduction = d.TotalDeduction,
                    NetSalary = d.NetSalary
                };
            })
            .OrderBy(r => CodeSortLength(r.EmployeeCode)).ThenBy(r => r.EmployeeCode)
            .ToList();

        return new PayrollReportDto
        {
            PayrollId = payroll.Id,
            PayrollMonth = payroll.PayrollMonth,
            PayrollYear = payroll.PayrollYear,
            Status = payroll.Status,
            TotalEmployees = rows.Count,
            TotalGross = rows.Sum(r => r.GrossSalary),
            TotalDeduction = rows.Sum(r => r.TotalDeduction),
            TotalNet = rows.Sum(r => r.NetSalary),
            Rows = rows,
            DepartmentSummary = rows
                .GroupBy(r => r.DepartmentName ?? "-")
                .Select(g => new PayrollDepartmentSummaryDto
                {
                    DepartmentName = g.Key,
                    Employees = g.Count(),
                    TotalGross = g.Sum(r => r.GrossSalary),
                    TotalDeduction = g.Sum(r => r.TotalDeduction),
                    TotalNet = g.Sum(r => r.NetSalary)
                })
                .OrderBy(s => s.DepartmentName)
                .ToList()
        };
    }

    // ---------------- Private Helpers ----------------

    private static string ParseDayStatus(string value)
    {
        if (Enum.TryParse<AttendanceDayStatus>(value, true, out var parsed) && Enum.IsDefined(typeof(AttendanceDayStatus), parsed))
            return parsed.ToString();

        throw new BadRequestException("Invalid Day Status. Allowed: " + string.Join(", ", Enum.GetNames<AttendanceDayStatus>()) + ".");
    }

    private static string ParseLeaveStatus(string value)
    {
        if (Enum.TryParse<LeaveStatus>(value, true, out var parsed) && Enum.IsDefined(typeof(LeaveStatus), parsed))
            return parsed.ToString();

        throw new BadRequestException("Invalid Status. Allowed: " + string.Join(", ", Enum.GetNames<LeaveStatus>()) + ".");
    }
}

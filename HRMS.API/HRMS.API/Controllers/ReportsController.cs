using HRMS.API.Helpers;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Reports;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private static bool IsCsv(string? format) => string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);

    private IActionResult Respond<T>(List<T> rows, string? format, string fileName)
    {
        if (IsCsv(format))
            return File(CsvExporter.ToCsv(rows), "text/csv", $"{fileName}_{DateTime.Now:yyyyMMdd_HHmm}.csv");

        return Ok(ApiResponse<List<T>>.SuccessResponse(rows));
    }

    // GET: api/reports/employees?departmentId=1&isActive=true&format=csv
    [HttpGet("employees")]
    public async Task<IActionResult> Employees([FromQuery] EmployeeReportFilterDto filter)
        => Respond(await _reportService.GetEmployeeReportAsync(filter), filter.Format, "employee_report");

    // GET: api/reports/attendance/monthly?year=2026&month=10&departmentId=1
    [HttpGet("attendance/monthly")]
    public async Task<IActionResult> MonthlyAttendance([FromQuery] MonthlyAttendanceFilterDto filter)
        => Respond(await _reportService.GetMonthlyAttendanceAsync(filter), filter.Format, $"attendance_{filter.Year}_{filter.Month:D2}");

    // GET: api/reports/attendance/daily?date=2026-10-20&dayStatus=Absent
    [HttpGet("attendance/daily")]
    public async Task<IActionResult> DailyAttendance([FromQuery] DailyAttendanceFilterDto filter)
        => Respond(await _reportService.GetDailyAttendanceAsync(filter), filter.Format, $"daily_attendance_{filter.Date:yyyyMMdd}");

    // GET: api/reports/late?fromDate=2026-10-01&toDate=2026-10-31
    [HttpGet("late")]
    public async Task<IActionResult> Late([FromQuery] LateReportFilterDto filter)
        => Respond(await _reportService.GetLateReportAsync(filter), filter.Format, "late_report");

    // GET: api/reports/overtime?fromDate=2026-10-01&toDate=2026-10-31
    [HttpGet("overtime")]
    public async Task<IActionResult> Overtime([FromQuery] OvertimeReportFilterDto filter)
        => Respond(await _reportService.GetOvertimeReportAsync(filter), filter.Format, "overtime_report");

    // GET: api/reports/leave?fromDate=2026-10-01&toDate=2026-10-31&status=Approved
    [HttpGet("leave")]
    public async Task<IActionResult> Leave([FromQuery] LeaveReportFilterDto filter)
        => Respond(await _reportService.GetLeaveReportAsync(filter), filter.Format, "leave_report");

    // GET: api/reports/payroll?payrollId=1   (CSV contains only the employee rows)
    [HttpGet("payroll")]
    public async Task<IActionResult> Payroll([FromQuery] PayrollReportFilterDto filter)
    {
        var report = await _reportService.GetPayrollReportAsync(filter);

        if (IsCsv(filter.Format))
            return File(CsvExporter.ToCsv(report.Rows), "text/csv",
                $"payroll_{report.PayrollYear}_{report.PayrollMonth:D2}.csv");

        return Ok(ApiResponse<PayrollReportDto>.SuccessResponse(report));
    }
}

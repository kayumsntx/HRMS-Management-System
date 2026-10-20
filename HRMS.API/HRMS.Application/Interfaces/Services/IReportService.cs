using HRMS.Application.DTOs.Reports;

namespace HRMS.Application.Interfaces.Services;

public interface IReportService
{
    Task<List<EmployeeReportRowDto>> GetEmployeeReportAsync(EmployeeReportFilterDto filter);
    Task<List<MonthlyAttendanceRowDto>> GetMonthlyAttendanceAsync(MonthlyAttendanceFilterDto filter);
    Task<List<DailyAttendanceRowDto>> GetDailyAttendanceAsync(DailyAttendanceFilterDto filter);
    Task<List<LateReportRowDto>> GetLateReportAsync(LateReportFilterDto filter);
    Task<List<OvertimeReportRowDto>> GetOvertimeReportAsync(OvertimeReportFilterDto filter);
    Task<List<LeaveReportRowDto>> GetLeaveReportAsync(LeaveReportFilterDto filter);
    Task<PayrollReportDto> GetPayrollReportAsync(PayrollReportFilterDto filter);
}

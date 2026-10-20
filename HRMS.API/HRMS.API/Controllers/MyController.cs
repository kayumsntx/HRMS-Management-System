using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Employees;
using HRMS.Application.DTOs.Holidays;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.DTOs.MySpace;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Self-service: every endpoint works ONLY on the logged-in user's own employee record.
[Authorize]
[ApiController]
[Route("api/my")]
public class MyController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly IEmployeeService _employeeService;
    private readonly IAttendanceService _attendanceService;
    private readonly ILeaveService _leaveService;
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly ILeaveTypeService _leaveTypeService;
    private readonly IHolidayService _holidayService;
    private readonly IMyPayslipService _payslipService;
    private readonly ITeamLeaveService _teamLeaveService;

    public MyController(
        ICurrentUserService currentUser,
        IEmployeeService employeeService,
        IAttendanceService attendanceService,
        ILeaveService leaveService,
        ILeaveBalanceService leaveBalanceService,
        ILeaveTypeService leaveTypeService,
        IHolidayService holidayService,
        IMyPayslipService payslipService,
        ITeamLeaveService teamLeaveService)
    {
        _currentUser = currentUser;
        _employeeService = employeeService;
        _attendanceService = attendanceService;
        _leaveService = leaveService;
        _leaveBalanceService = leaveBalanceService;
        _leaveTypeService = leaveTypeService;
        _holidayService = holidayService;
        _payslipService = payslipService;
        _teamLeaveService = teamLeaveService;
    }

    private int MyEmployeeId => _currentUser.EmployeeId
        ?? throw new BadRequestException("Your user account is not linked to an employee record. Please contact HR.");

    // ---------------- Profile ----------------

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var result = await _employeeService.GetByIdAsync(MyEmployeeId);
        return Ok(ApiResponse<EmployeeResponseDto>.SuccessResponse(result));
    }

    // ---------------- Attendance ----------------

    // GET: api/my/attendance?fromDate=2026-10-01&toDate=2026-10-31&dayStatus=Late
    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance([FromQuery] AttendanceFilterDto filter)
    {
        filter.EmployeeId = MyEmployeeId;   // forced: cannot look at anybody else
        filter.DepartmentId = null;

        var result = await _attendanceService.GetAsync(filter);
        return Ok(ApiResponse<List<AttendanceResponseDto>>.SuccessResponse(result));
    }

    // GET: api/my/attendance/summary?year=2026&month=10
    [HttpGet("attendance/summary")]
    public async Task<IActionResult> AttendanceSummary([FromQuery] int year, [FromQuery] int month)
    {
        var result = await _attendanceService.GetMonthlySummaryAsync(MyEmployeeId, year, month);
        return Ok(ApiResponse<AttendanceSummaryDto>.SuccessResponse(result));
    }

    // ---------------- Leave ----------------

    [HttpGet("leaves")]
    public async Task<IActionResult> Leaves()
    {
        var result = await _leaveService.GetByEmployeeIdAsync(MyEmployeeId);
        return Ok(ApiResponse<List<LeaveResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("leave-balances")]
    public async Task<IActionResult> LeaveBalances([FromQuery] int? year)
    {
        var result = await _leaveBalanceService.GetByEmployeeIdAsync(MyEmployeeId, year ?? DateTime.Now.Year);
        return Ok(ApiResponse<List<LeaveBalanceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("leave-types")]
    public async Task<IActionResult> LeaveTypes()
    {
        var all = await _leaveTypeService.GetAllAsync();
        return Ok(ApiResponse<List<LeaveTypeResponseDto>>.SuccessResponse(all.Where(l => l.IsActive).ToList()));
    }

    [HttpPost("leaves/apply")]
    public async Task<IActionResult> ApplyLeave([FromBody] MyLeaveApplyDto dto)
    {
        var result = await _leaveService.ApplyAsync(new LeaveApplyDto
        {
            EmployeeId = MyEmployeeId,   // forced: apply only for myself
            LeaveTypeId = dto.LeaveTypeId,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            Reason = dto.Reason
        });

        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave applied successfully."));
    }

    [HttpPost("leaves/{id:int}/cancel")]
    public async Task<IActionResult> CancelLeave(int id)
    {
        var leave = await _leaveService.GetByIdAsync(id);

        if (leave.EmployeeId != MyEmployeeId)
            throw new NotFoundException("EmployeeLeave", id);

        var result = await _leaveService.CancelAsync(id);
        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave cancelled."));
    }

    // ---------------- Holidays ----------------

    [HttpGet("holidays")]
    public async Task<IActionResult> Holidays([FromQuery] int? year)
    {
        var all = await _holidayService.GetAllAsync(year);
        return Ok(ApiResponse<List<HolidayResponseDto>>.SuccessResponse(all.Where(h => h.IsActive).ToList()));
    }

    // ---------------- Payslips ----------------

    [HttpGet("payslips")]
    public async Task<IActionResult> Payslips()
    {
        var result = await _payslipService.GetListAsync(MyEmployeeId);
        return Ok(ApiResponse<List<MyPayslipSummaryDto>>.SuccessResponse(result));
    }

    [HttpGet("payslips/{payrollId:int}")]
    public async Task<IActionResult> Payslip(int payrollId)
    {
        var result = await _payslipService.GetAsync(MyEmployeeId, payrollId);
        return Ok(ApiResponse<PayslipDto>.SuccessResponse(result));
    }

    // ---------------- Team (managers: direct reports only) ----------------

    // GET: api/my/team/leaves?status=Pending
    [Authorize(Roles = "Manager,Admin,HR")]
    [HttpGet("team/leaves")]
    public async Task<IActionResult> TeamLeaves([FromQuery] string? status)
    {
        var result = await _teamLeaveService.GetTeamLeavesAsync(MyEmployeeId, status);
        return Ok(ApiResponse<List<LeaveResponseDto>>.SuccessResponse(result));
    }

    [Authorize(Roles = "Manager,Admin,HR")]
    [HttpPost("team/leaves/{id:int}/approve")]
    public async Task<IActionResult> ApproveTeamLeave(int id)
    {
        var result = await _teamLeaveService.ApproveAsync(MyEmployeeId, id);
        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave approved successfully."));
    }

    [Authorize(Roles = "Manager,Admin,HR")]
    [HttpPost("team/leaves/{id:int}/reject")]
    public async Task<IActionResult> RejectTeamLeave(int id, [FromBody] LeaveRejectDto dto)
    {
        var result = await _teamLeaveService.RejectAsync(MyEmployeeId, id, dto);
        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave rejected."));
    }
}

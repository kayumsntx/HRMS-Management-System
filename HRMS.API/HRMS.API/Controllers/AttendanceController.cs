using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // GET: api/attendance?employeeId=1&departmentId=1&fromDate=2026-10-01&toDate=2026-10-31&dayStatus=Late
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AttendanceFilterDto filter)
    {
        var result = await _attendanceService.GetAsync(filter);
        return Ok(ApiResponse<List<AttendanceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _attendanceService.GetByIdAsync(id);
        return Ok(ApiResponse<AttendanceResponseDto>.SuccessResponse(result));
    }

    // GET: api/attendance/summary?employeeId=1&year=2026&month=10
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] int employeeId, [FromQuery] int year, [FromQuery] int month)
    {
        var result = await _attendanceService.GetMonthlySummaryAsync(employeeId, year, month);
        return Ok(ApiResponse<AttendanceSummaryDto>.SuccessResponse(result));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] AttendanceProcessDto dto)
    {
        var result = await _attendanceService.ProcessAsync(dto);
        return Ok(ApiResponse<AttendanceProcessResultDto>.SuccessResponse(result, "Attendance processed successfully."));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPut("{id:int}/correct")]
    public async Task<IActionResult> Correct(int id, [FromBody] AttendanceCorrectionDto dto)
    {
        var result = await _attendanceService.CorrectAsync(id, dto);
        return Ok(ApiResponse<AttendanceResponseDto>.SuccessResponse(result, "Attendance corrected successfully."));
    }
}

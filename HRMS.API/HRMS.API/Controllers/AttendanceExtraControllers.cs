using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Admin/HR (fallback policy).
[ApiController]
[Route("api/[controller]")]
public class ShortLeavesController : ControllerBase
{
    private readonly IShortLeaveService _service;

    public ShortLeavesController(IShortLeaveService service)
    {
        _service = service;
    }

    // GET: api/shortleaves?status=Pending&employeeId=1&fromDate=2026-10-01&toDate=2026-10-31
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] int? employeeId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        => Ok(ApiResponse<List<ShortLeaveResponseDto>>.SuccessResponse(await _service.GetAsync(status, employeeId, fromDate, toDate)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<ShortLeaveResponseDto>.SuccessResponse(await _service.GetByIdAsync(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShortLeaveCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<ShortLeaveResponseDto>.SuccessResponse(created, "Short leave created."));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => Ok(ApiResponse<ShortLeaveResponseDto>.SuccessResponse(await _service.ApproveAsync(id), "Short leave approved."));

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReasonDto dto)
        => Ok(ApiResponse<ShortLeaveResponseDto>.SuccessResponse(await _service.RejectAsync(id, dto), "Short leave rejected."));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(ApiResponse<ShortLeaveResponseDto>.SuccessResponse(await _service.CancelAsync(id), "Short leave cancelled."));
}

// Self-service short leave: works ONLY on the logged-in user's own employee record.
[Authorize]
[ApiController]
[Route("api/my/short-leaves")]
public class MyShortLeavesController : ControllerBase
{
    private readonly IShortLeaveService _service;
    private readonly ICurrentUserService _currentUser;

    public MyShortLeavesController(IShortLeaveService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private int MyEmployeeId => _currentUser.EmployeeId
        ?? throw new BadRequestException("Your user account is not linked to an employee record. Please contact HR.");

    [HttpGet]
    public async Task<IActionResult> Mine([FromQuery] string? status)
        => Ok(ApiResponse<List<ShortLeaveResponseDto>>.SuccessResponse(await _service.GetAsync(status, MyEmployeeId, null, null)));

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] MyShortLeaveCreateDto dto)
    {
        var created = await _service.CreateAsync(new ShortLeaveCreateDto
        {
            EmployeeId = MyEmployeeId,   // forced: only for myself
            LeaveDate = dto.LeaveDate,
            FromTime = dto.FromTime,
            ToTime = dto.ToTime,
            Reason = dto.Reason
        });

        return Ok(ApiResponse<ShortLeaveResponseDto>.SuccessResponse(created, "Short leave request submitted."));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item.EmployeeId != MyEmployeeId)
            throw new NotFoundException("ShortLeave", id);

        return Ok(ApiResponse<ShortLeaveResponseDto>.SuccessResponse(await _service.CancelAsync(id), "Short leave request cancelled."));
    }
}

// Admin/HR (fallback policy).
[ApiController]
[Route("api/[controller]")]
public class AttendanceLocksController : ControllerBase
{
    private readonly IAttendanceLockService _service;

    public AttendanceLocksController(IAttendanceLockService service)
    {
        _service = service;
    }

    // GET: api/attendancelocks?year=2026
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? year)
        => Ok(ApiResponse<List<AttendanceLockResponseDto>>.SuccessResponse(await _service.GetAsync(year)));

    // Approve (lock) a finished month.
    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromBody] AttendanceLockDto dto)
        => Ok(ApiResponse<AttendanceLockResponseDto>.SuccessResponse(await _service.ApproveAsync(dto), "Attendance approved and locked."));

    [HttpPost("{id:int}/reopen")]
    public async Task<IActionResult> Reopen(int id, [FromBody] ReasonDto dto)
        => Ok(ApiResponse<AttendanceLockResponseDto>.SuccessResponse(await _service.ReopenAsync(id, dto), "Attendance reopened."));
}

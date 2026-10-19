using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    // GET: api/leave?status=Pending
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var result = await _leaveService.GetAllAsync(status);
        return Ok(ApiResponse<List<LeaveResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId)
    {
        var result = await _leaveService.GetByEmployeeIdAsync(employeeId);
        return Ok(ApiResponse<List<LeaveResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _leaveService.GetByIdAsync(id);
        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result));
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] LeaveApplyDto dto)
    {
        var result = await _leaveService.ApplyAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave applied successfully."));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _leaveService.ApproveAsync(id);
        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave approved successfully."));
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] LeaveRejectDto dto)
    {
        var result = await _leaveService.RejectAsync(id, dto);
        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave rejected."));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _leaveService.CancelAsync(id);
        return Ok(ApiResponse<LeaveResponseDto>.SuccessResponse(result, "Leave cancelled."));
    }
}

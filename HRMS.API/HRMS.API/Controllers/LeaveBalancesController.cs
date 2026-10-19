using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveBalancesController : ControllerBase
{
    private readonly ILeaveBalanceService _service;

    public LeaveBalancesController(ILeaveBalanceService service)
    {
        _service = service;
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId, [FromQuery] int? year)
    {
        var result = await _service.GetByEmployeeIdAsync(employeeId, year);
        return Ok(ApiResponse<List<LeaveBalanceResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<LeaveBalanceResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeaveBalanceCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<LeaveBalanceResponseDto>.SuccessResponse(created, "Leave balance created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeaveBalanceUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<LeaveBalanceResponseDto>.SuccessResponse(updated, "Leave balance updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Leave balance deleted successfully."));
    }
}

using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.ShiftAssignments;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftAssignmentsController : ControllerBase
{
    private readonly IShiftAssignmentService _service;

    public ShiftAssignmentsController(IShiftAssignmentService service)
    {
        _service = service;
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId)
    {
        var result = await _service.GetByEmployeeIdAsync(employeeId);
        return Ok(ApiResponse<List<ShiftAssignmentResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ShiftAssignmentResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShiftAssignmentCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<ShiftAssignmentResponseDto>.SuccessResponse(created, "Shift assigned successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShiftAssignmentCreateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<ShiftAssignmentResponseDto>.SuccessResponse(updated, "Shift assignment updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Shift assignment deleted successfully."));
    }
}

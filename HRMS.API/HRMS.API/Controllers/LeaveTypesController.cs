using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveTypesController : ControllerBase
{
    private readonly ILeaveTypeService _service;

    public LeaveTypesController(ILeaveTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<List<LeaveTypeResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<LeaveTypeResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeaveTypeCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<LeaveTypeResponseDto>.SuccessResponse(created, "Leave Type created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeaveTypeUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<LeaveTypeResponseDto>.SuccessResponse(updated, "Leave Type updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Leave Type deleted successfully."));
    }
}

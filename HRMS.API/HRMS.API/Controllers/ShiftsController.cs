using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Shifts;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _shiftService.GetAllAsync();
        return Ok(ApiResponse<List<ShiftResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _shiftService.GetByIdAsync(id);
        return Ok(ApiResponse<ShiftResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShiftCreateDto dto)
    {
        var created = await _shiftService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<ShiftResponseDto>.SuccessResponse(created, "Shift created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ShiftUpdateDto dto)
    {
        var updated = await _shiftService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ShiftResponseDto>.SuccessResponse(updated, "Shift updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _shiftService.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Shift deleted successfully."));
    }
}

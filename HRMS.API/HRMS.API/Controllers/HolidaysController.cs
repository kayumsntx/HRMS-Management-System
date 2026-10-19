using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Holidays;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HolidaysController : ControllerBase
{
    private readonly IHolidayService _service;

    public HolidaysController(IHolidayService service)
    {
        _service = service;
    }

    // GET: api/holidays?year=2026   (default = current year)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? year)
    {
        var result = await _service.GetAllAsync(year);
        return Ok(ApiResponse<List<HolidayResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<HolidayResponseDto>.SuccessResponse(result));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HolidayCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<HolidayResponseDto>.SuccessResponse(created, "Holiday created successfully."));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HolidayUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<HolidayResponseDto>.SuccessResponse(updated, "Holiday updated successfully."));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Holiday deleted successfully."));
    }
}

using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Biometric;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/[controller]")]
public class BiometricController : ControllerBase
{
    private readonly IBiometricPunchService _service;

    public BiometricController(IBiometricPunchService service)
    {
        _service = service;
    }

    // GET: api/biometric/punches?employeeId=1&deviceId=1&fromDate=2026-10-01&toDate=2026-10-31&isProcessed=false
    [HttpGet("punches")]
    public async Task<IActionResult> GetPunches(
        [FromQuery] int? employeeId, [FromQuery] int? deviceId,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] bool? isProcessed)
    {
        var result = await _service.GetAsync(employeeId, deviceId, fromDate, toDate, isProcessed);
        return Ok(ApiResponse<List<BiometricPunchResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("punches/{id:int}")]
    public async Task<IActionResult> GetPunchById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<BiometricPunchResponseDto>.SuccessResponse(result));
    }

    [HttpPost("punches")]
    public async Task<IActionResult> CreatePunch([FromBody] BiometricPunchCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetPunchById), new { id = created.Id },
            ApiResponse<BiometricPunchResponseDto>.SuccessResponse(created, "Punch recorded successfully."));
    }

    [HttpPost("punches/bulk")]
    public async Task<IActionResult> BulkCreatePunches([FromBody] BiometricPunchBulkCreateDto dto)
    {
        var result = await _service.BulkCreateAsync(dto);
        return Ok(ApiResponse<BiometricPunchBulkResultDto>.SuccessResponse(result, "Bulk punch import completed."));
    }

    [HttpDelete("punches/{id:int}")]
    public async Task<IActionResult> DeletePunch(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Punch deleted successfully."));
    }
}

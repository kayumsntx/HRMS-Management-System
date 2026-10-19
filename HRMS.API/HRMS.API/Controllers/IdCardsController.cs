using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.IdCards;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/[controller]")]
public class IdCardsController : ControllerBase
{
    private readonly IIdCardService _service;

    public IdCardsController(IIdCardService service)
    {
        _service = service;
    }

    // GET: api/idcards?status=Active
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var result = await _service.GetAllAsync(status);
        return Ok(ApiResponse<List<IdCardResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId)
    {
        var result = await _service.GetByEmployeeIdAsync(employeeId);
        return Ok(ApiResponse<List<IdCardResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<IdCardResponseDto>.SuccessResponse(result));
    }

    // Everything needed to draw the physical card (name, photo, QR text, etc.)
    [HttpGet("{id:int}/preview")]
    public async Task<IActionResult> Preview(int id)
    {
        var result = await _service.GetPreviewAsync(id);
        return Ok(ApiResponse<IdCardPreviewDto>.SuccessResponse(result));
    }

    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IdCardIssueDto dto)
    {
        var result = await _service.IssueAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<IdCardResponseDto>.SuccessResponse(result, "ID card issued successfully."));
    }

    [HttpPost("reissue")]
    public async Task<IActionResult> Reissue([FromBody] IdCardReissueDto dto)
    {
        var result = await _service.ReissueAsync(dto);
        return Ok(ApiResponse<IdCardResponseDto>.SuccessResponse(result, "ID card re-issued successfully."));
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] IdCardStatusDto dto)
    {
        var result = await _service.ChangeStatusAsync(id, dto);
        return Ok(ApiResponse<IdCardResponseDto>.SuccessResponse(result, "ID card status updated."));
    }

    [HttpPost("{id:int}/print")]
    public async Task<IActionResult> RegisterPrint(int id)
    {
        var result = await _service.RegisterPrintAsync(id);
        return Ok(ApiResponse<IdCardResponseDto>.SuccessResponse(result, "Print recorded."));
    }
}

using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Career;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CareerController : ControllerBase
{
    private readonly ICareerService _service;

    public CareerController(ICareerService service)
    {
        _service = service;
    }

    // GET: api/career?employeeId=1&changeType=Promotion&status=Pending
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? employeeId, [FromQuery] string? changeType, [FromQuery] string? status)
        => Ok(ApiResponse<List<CareerChangeResponseDto>>.SuccessResponse(await _service.GetAsync(employeeId, changeType, status)));

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
        => Ok(ApiResponse<List<CareerChangeResponseDto>>.SuccessResponse(await _service.GetByEmployeeAsync(employeeId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<CareerChangeResponseDto>.SuccessResponse(await _service.GetByIdAsync(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CareerChangeCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CareerChangeResponseDto>.SuccessResponse(created, "Career change request created."));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => Ok(ApiResponse<CareerChangeResponseDto>.SuccessResponse(await _service.ApproveAsync(id), "Career change approved."));

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReasonDto dto)
        => Ok(ApiResponse<CareerChangeResponseDto>.SuccessResponse(await _service.RejectAsync(id, dto), "Career change rejected."));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(ApiResponse<CareerChangeResponseDto>.SuccessResponse(await _service.CancelAsync(id), "Career change cancelled."));

    // Applies all approved future-dated changes whose effective date has arrived.
    [HttpPost("apply-due")]
    public async Task<IActionResult> ApplyDue()
    {
        var count = await _service.ApplyDueAsync();
        return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} career change(s) applied."));
    }
}

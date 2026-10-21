using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Requisitions;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Admin/HR (fallback policy). Employees use /api/my/requisitions.
[ApiController]
[Route("api/[controller]")]
public class RequisitionsController : ControllerBase
{
    private readonly IRequisitionService _service;

    public RequisitionsController(IRequisitionService service)
    {
        _service = service;
    }

    // GET: api/requisitions?status=Pending&typeId=1&employeeId=1&fromDate=2026-10-01&toDate=2026-10-31
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] int? typeId, [FromQuery] int? employeeId,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        => Ok(ApiResponse<List<RequisitionResponseDto>>.SuccessResponse(await _service.GetAsync(status, typeId, employeeId, fromDate, toDate)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<RequisitionResponseDto>.SuccessResponse(await _service.GetByIdAsync(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RequisitionCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<RequisitionResponseDto>.SuccessResponse(created, "Requisition created successfully."));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => Ok(ApiResponse<RequisitionResponseDto>.SuccessResponse(await _service.ApproveAsync(id), "Requisition approved."));

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReasonDto dto)
        => Ok(ApiResponse<RequisitionResponseDto>.SuccessResponse(await _service.RejectAsync(id, dto), "Requisition rejected."));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(ApiResponse<RequisitionResponseDto>.SuccessResponse(await _service.CancelAsync(id), "Requisition cancelled."));
}

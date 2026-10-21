using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Separations;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Admin/HR (fallback policy). Employees resign through /api/my/resignation.
[ApiController]
[Route("api/[controller]")]
public class SeparationsController : ControllerBase
{
    private readonly ISeparationService _service;

    public SeparationsController(ISeparationService service)
    {
        _service = service;
    }

    // GET: api/separations?status=Pending&type=Resignation
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? type)
        => Ok(ApiResponse<List<SeparationResponseDto>>.SuccessResponse(await _service.GetAllAsync(status, type)));

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
        => Ok(ApiResponse<List<SeparationResponseDto>>.SuccessResponse(await _service.GetByEmployeeAsync(employeeId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.GetByIdAsync(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SeparationCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Separation.Id }, ApiResponse<SeparationDetailDto>.SuccessResponse(created, "Separation request created."));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] SeparationApproveDto dto)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.ApproveAsync(id, dto), "Separation approved."));

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReasonDto dto)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.RejectAsync(id, dto), "Separation rejected."));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.CancelAsync(id), "Separation cancelled."));

    [HttpPut("{id:int}/exit-interview")]
    public async Task<IActionResult> ExitInterview(int id, [FromBody] ExitInterviewDto dto)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.SaveExitInterviewAsync(id, dto), "Exit interview saved."));

    [HttpPost("{id:int}/clearances")]
    public async Task<IActionResult> AddClearance(int id, [FromBody] ClearanceCreateDto dto)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.AddClearanceAsync(id, dto), "Clearance item added."));

    [HttpPut("clearances/{clearanceId:int}")]
    public async Task<IActionResult> UpdateClearance(int clearanceId, [FromBody] ClearanceUpdateDto dto)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.UpdateClearanceAsync(clearanceId, dto), "Clearance item updated."));

    [HttpDelete("clearances/{clearanceId:int}")]
    public async Task<IActionResult> DeleteClearance(int clearanceId)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.DeleteClearanceAsync(clearanceId), "Clearance item deleted."));

    // Calculates and saves (or recalculates) the draft settlement.
    [HttpPut("{id:int}/settlement")]
    public async Task<IActionResult> SaveSettlement(int id, [FromBody] SettlementSaveDto dto)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.SaveSettlementAsync(id, dto), "Settlement saved."));

    [HttpPost("{id:int}/settlement/approve")]
    public async Task<IActionResult> ApproveSettlement(int id)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.ApproveSettlementAsync(id), "Settlement approved."));

    [HttpPost("{id:int}/settlement/pay")]
    public async Task<IActionResult> PaySettlement(int id)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.PaySettlementAsync(id), "Settlement marked as paid."));

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _service.CompleteAsync(id), "Separation completed."));
}

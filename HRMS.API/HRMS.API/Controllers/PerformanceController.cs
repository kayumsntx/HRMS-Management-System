using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Performance;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Periods and KPIs are managed through /api/performanceperiods and /api/kpis.
[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceService _service;

    public PerformanceController(IPerformanceService service)
    {
        _service = service;
    }

    // ---------------- Goals ----------------

    [HttpGet("goals")]
    public async Task<IActionResult> GetGoals([FromQuery] int? employeeId, [FromQuery] int? periodId, [FromQuery] string? status)
        => Ok(ApiResponse<List<GoalResponseDto>>.SuccessResponse(await _service.GetGoalsAsync(employeeId, periodId, status)));

    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] GoalCreateDto dto)
        => Ok(ApiResponse<GoalResponseDto>.SuccessResponse(await _service.CreateGoalAsync(dto), "Goal created."));

    [HttpPut("goals/{id:int}")]
    public async Task<IActionResult> UpdateGoal(int id, [FromBody] GoalUpdateDto dto)
        => Ok(ApiResponse<GoalResponseDto>.SuccessResponse(await _service.UpdateGoalAsync(id, dto), "Goal updated."));

    [HttpDelete("goals/{id:int}")]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        await _service.DeleteGoalAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Goal cancelled."));
    }

    // ---------------- Evaluations ----------------

    [HttpGet("evaluations")]
    public async Task<IActionResult> GetEvaluations([FromQuery] int? employeeId, [FromQuery] int? periodId, [FromQuery] string? status)
        => Ok(ApiResponse<List<EvaluationResponseDto>>.SuccessResponse(await _service.GetEvaluationsAsync(employeeId, periodId, status)));

    [HttpGet("evaluations/{id:int}")]
    public async Task<IActionResult> GetEvaluationById(int id)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.GetEvaluationByIdAsync(id)));

    // Performance history of one employee (all periods).
    [HttpGet("evaluations/employee/{employeeId:int}/history")]
    public async Task<IActionResult> History(int employeeId)
        => Ok(ApiResponse<List<EvaluationResponseDto>>.SuccessResponse(await _service.GetHistoryAsync(employeeId, false)));

    [HttpPost("evaluations")]
    public async Task<IActionResult> CreateEvaluation([FromBody] EvaluationCreateDto dto)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.CreateEvaluationAsync(dto), "Evaluation created."));

    [HttpPut("evaluations/{id:int}")]
    public async Task<IActionResult> UpdateEvaluation(int id, [FromBody] EvaluationUpdateDto dto)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.UpdateEvaluationAsync(id, dto), "Evaluation updated."));

    [HttpPost("evaluations/{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.SubmitAsync(id), "Evaluation submitted."));

    [HttpPost("evaluations/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.ApproveAsync(id), "Evaluation approved."));

    [HttpPost("evaluations/{id:int}/reopen")]
    public async Task<IActionResult> Reopen(int id)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.ReopenAsync(id), "Evaluation re-opened."));

    [HttpDelete("evaluations/{id:int}")]
    public async Task<IActionResult> DeleteEvaluation(int id)
    {
        await _service.DeleteEvaluationAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Evaluation deleted."));
    }
}

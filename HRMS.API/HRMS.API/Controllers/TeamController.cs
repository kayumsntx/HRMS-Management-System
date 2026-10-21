using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Performance;
using HRMS.Application.DTOs.Team;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Leave approvals of the team are already in MyController (api/my/team/leaves).
[Authorize(Roles = "Manager,Admin,HR")]
[ApiController]
[Route("api/my/team")]
public class TeamController : ControllerBase
{
    private readonly ITeamService _service;
    private readonly ICurrentUserService _currentUser;

    public TeamController(ITeamService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private int MyEmployeeId => _currentUser.EmployeeId
        ?? throw new BadRequestException("Your user account is not linked to an employee record. Please contact HR.");

    [HttpGet("members")]
    public async Task<IActionResult> Members()
        => Ok(ApiResponse<List<TeamMemberDto>>.SuccessResponse(await _service.GetMembersAsync(MyEmployeeId)));

    // GET: api/my/team/attendance/summary?year=2026&month=10
    [HttpGet("attendance/summary")]
    public async Task<IActionResult> AttendanceSummary([FromQuery] int year, [FromQuery] int month)
        => Ok(ApiResponse<List<AttendanceSummaryDto>>.SuccessResponse(await _service.GetAttendanceSummaryAsync(MyEmployeeId, year, month)));

    [HttpGet("goals")]
    public async Task<IActionResult> Goals([FromQuery] int? periodId, [FromQuery] string? status)
        => Ok(ApiResponse<List<GoalResponseDto>>.SuccessResponse(await _service.GetGoalsAsync(MyEmployeeId, periodId, status)));

    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] GoalCreateDto dto)
        => Ok(ApiResponse<GoalResponseDto>.SuccessResponse(await _service.CreateGoalAsync(MyEmployeeId, dto), "Goal created."));

    [HttpGet("evaluations")]
    public async Task<IActionResult> Evaluations([FromQuery] int? periodId, [FromQuery] string? status)
        => Ok(ApiResponse<List<EvaluationResponseDto>>.SuccessResponse(await _service.GetEvaluationsAsync(MyEmployeeId, periodId, status)));

    [HttpPost("evaluations")]
    public async Task<IActionResult> CreateEvaluation([FromBody] EvaluationCreateDto dto)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.CreateEvaluationAsync(MyEmployeeId, dto), "Evaluation created."));

    [HttpPut("evaluations/{id:int}")]
    public async Task<IActionResult> UpdateEvaluation(int id, [FromBody] EvaluationUpdateDto dto)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.UpdateEvaluationAsync(MyEmployeeId, id, dto), "Evaluation updated."));

    [HttpPost("evaluations/{id:int}/submit")]
    public async Task<IActionResult> SubmitEvaluation(int id)
        => Ok(ApiResponse<EvaluationResponseDto>.SuccessResponse(await _service.SubmitEvaluationAsync(MyEmployeeId, id), "Evaluation submitted."));
}

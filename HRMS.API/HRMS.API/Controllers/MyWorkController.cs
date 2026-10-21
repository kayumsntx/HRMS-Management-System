using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Career;
using HRMS.Application.DTOs.Masters;
using HRMS.Application.DTOs.Performance;
using HRMS.Application.DTOs.Requisitions;
using HRMS.Application.DTOs.Separations;
using HRMS.Application.DTOs.Training;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Every endpoint works ONLY on the logged-in user's own employee record.
[Authorize]
[ApiController]
[Route("api/my")]
public class MyWorkController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly IRequisitionService _requisitions;
    private readonly IRequisitionTypeService _requisitionTypes;
    private readonly ICareerService _career;
    private readonly ISeparationService _separations;
    private readonly IPerformanceService _performance;
    private readonly ITrainingService _training;

    public MyWorkController(
        ICurrentUserService currentUser,
        IRequisitionService requisitions,
        IRequisitionTypeService requisitionTypes,
        ICareerService career,
        ISeparationService separations,
        IPerformanceService performance,
        ITrainingService training)
    {
        _currentUser = currentUser;
        _requisitions = requisitions;
        _requisitionTypes = requisitionTypes;
        _career = career;
        _separations = separations;
        _performance = performance;
        _training = training;
    }

    private int MyEmployeeId => _currentUser.EmployeeId
        ?? throw new BadRequestException("Your user account is not linked to an employee record. Please contact HR.");

    // ---------------- Requisitions ----------------

    [HttpGet("requisition-types")]
    public async Task<IActionResult> RequisitionTypes()
    {
        var all = await _requisitionTypes.GetAllAsync();
        return Ok(ApiResponse<List<RequisitionTypeResponseDto>>.SuccessResponse(all.Where(t => t.IsActive).ToList()));
    }

    [HttpGet("requisitions")]
    public async Task<IActionResult> Requisitions([FromQuery] string? status)
        => Ok(ApiResponse<List<RequisitionResponseDto>>.SuccessResponse(await _requisitions.GetAsync(status, null, MyEmployeeId, null, null)));

    [HttpPost("requisitions")]
    public async Task<IActionResult> CreateRequisition([FromBody] MyRequisitionCreateDto dto)
    {
        var created = await _requisitions.CreateAsync(new RequisitionCreateDto
        {
            EmployeeId = MyEmployeeId,   // forced: only for myself
            RequisitionTypeId = dto.RequisitionTypeId,
            RequisitionDate = dto.RequisitionDate,
            Purpose = dto.Purpose,
            Items = dto.Items
        });

        return Ok(ApiResponse<RequisitionResponseDto>.SuccessResponse(created, "Requisition submitted."));
    }

    [HttpPost("requisitions/{id:int}/cancel")]
    public async Task<IActionResult> CancelRequisition(int id)
    {
        var requisition = await _requisitions.GetByIdAsync(id);
        if (requisition.EmployeeId != MyEmployeeId)
            throw new NotFoundException("Requisition", id);

        return Ok(ApiResponse<RequisitionResponseDto>.SuccessResponse(await _requisitions.CancelAsync(id), "Requisition cancelled."));
    }

    // ---------------- Career ----------------

    // Only approved career changes are shown to the employee.
    [HttpGet("career")]
    public async Task<IActionResult> Career()
    {
        var all = await _career.GetByEmployeeAsync(MyEmployeeId);
        return Ok(ApiResponse<List<CareerChangeResponseDto>>.SuccessResponse(all.Where(c => c.Status == "Approved").ToList()));
    }

    // ---------------- Resignation ----------------

    [HttpGet("separations")]
    public async Task<IActionResult> MySeparations()
        => Ok(ApiResponse<List<SeparationResponseDto>>.SuccessResponse(await _separations.GetByEmployeeAsync(MyEmployeeId)));

    [HttpPost("resignation")]
    public async Task<IActionResult> Resign([FromBody] MySeparationCreateDto dto)
        => Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _separations.CreateForSelfAsync(MyEmployeeId, dto), "Resignation request submitted."));

    [HttpPost("resignation/{id:int}/cancel")]
    public async Task<IActionResult> CancelResignation(int id)
    {
        var detail = await _separations.GetByIdAsync(id);

        if (detail.Separation.EmployeeId != MyEmployeeId)
            throw new NotFoundException("EmployeeSeparation", id);

        if (detail.Separation.Status != "Pending")
            throw new BadRequestException("Only a Pending request can be withdrawn. Please contact HR.");

        return Ok(ApiResponse<SeparationDetailDto>.SuccessResponse(await _separations.CancelAsync(id), "Resignation request withdrawn."));
    }

    // ---------------- Performance ----------------

    [HttpGet("goals")]
    public async Task<IActionResult> Goals([FromQuery] int? periodId, [FromQuery] string? status)
        => Ok(ApiResponse<List<GoalResponseDto>>.SuccessResponse(await _performance.GetGoalsAsync(MyEmployeeId, periodId, status)));

    [HttpPut("goals/{id:int}/progress")]
    public async Task<IActionResult> UpdateGoalProgress(int id, [FromBody] GoalProgressDto dto)
        => Ok(ApiResponse<GoalResponseDto>.SuccessResponse(await _performance.UpdateGoalProgressAsync(id, dto, MyEmployeeId), "Goal progress updated."));

    // Only approved evaluations are visible to the employee.
    [HttpGet("evaluations")]
    public async Task<IActionResult> Evaluations()
        => Ok(ApiResponse<List<EvaluationResponseDto>>.SuccessResponse(await _performance.GetHistoryAsync(MyEmployeeId, true)));

    // ---------------- Training ----------------

    [HttpGet("trainings")]
    public async Task<IActionResult> Trainings()
        => Ok(ApiResponse<List<TrainingHistoryDto>>.SuccessResponse(await _training.GetEmployeeHistoryAsync(MyEmployeeId)));
}

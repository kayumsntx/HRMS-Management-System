using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Training;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Programs and trainers are managed through /api/trainingprograms and /api/trainers.
[ApiController]
[Route("api/[controller]")]
public class TrainingController : ControllerBase
{
    private readonly ITrainingService _service;

    public TrainingController(ITrainingService service)
    {
        _service = service;
    }

    // GET: api/training/schedules?status=Planned&programId=1&fromDate=2026-10-01&toDate=2026-12-31
    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules([FromQuery] string? status, [FromQuery] int? programId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        => Ok(ApiResponse<List<TrainingScheduleResponseDto>>.SuccessResponse(await _service.GetSchedulesAsync(status, programId, fromDate, toDate)));

    [HttpGet("schedules/{id:int}")]
    public async Task<IActionResult> GetSchedule(int id)
        => Ok(ApiResponse<TrainingScheduleResponseDto>.SuccessResponse(await _service.GetScheduleByIdAsync(id)));

    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedule([FromBody] TrainingScheduleCreateDto dto)
        => Ok(ApiResponse<TrainingScheduleResponseDto>.SuccessResponse(await _service.CreateScheduleAsync(dto), "Training scheduled."));

    [HttpPut("schedules/{id:int}")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] TrainingScheduleUpdateDto dto)
        => Ok(ApiResponse<TrainingScheduleResponseDto>.SuccessResponse(await _service.UpdateScheduleAsync(id, dto), "Training schedule updated."));

    [HttpPost("schedules/{id:int}/start")]
    public async Task<IActionResult> Start(int id)
        => Ok(ApiResponse<TrainingScheduleResponseDto>.SuccessResponse(await _service.StartAsync(id), "Training started."));

    [HttpPost("schedules/{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
        => Ok(ApiResponse<TrainingScheduleResponseDto>.SuccessResponse(await _service.CompleteAsync(id), "Training completed."));

    [HttpPost("schedules/{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(ApiResponse<TrainingScheduleResponseDto>.SuccessResponse(await _service.CancelAsync(id), "Training cancelled."));

    // ---------------- Participants ----------------

    [HttpGet("schedules/{id:int}/participants")]
    public async Task<IActionResult> GetParticipants(int id)
        => Ok(ApiResponse<List<TrainingParticipantResponseDto>>.SuccessResponse(await _service.GetParticipantsAsync(id)));

    [HttpPost("schedules/{id:int}/participants")]
    public async Task<IActionResult> Enroll(int id, [FromBody] TrainingEnrollDto dto)
        => Ok(ApiResponse<List<TrainingParticipantResponseDto>>.SuccessResponse(await _service.EnrollAsync(id, dto), "Participants enrolled."));

    [HttpDelete("participants/{participantId:int}")]
    public async Task<IActionResult> Drop(int participantId)
        => Ok(ApiResponse<List<TrainingParticipantResponseDto>>.SuccessResponse(await _service.DropParticipantAsync(participantId), "Participant dropped."));

    // ---------------- Attendance ----------------

    // GET: api/training/schedules/1/attendance?date=2026-10-20
    [HttpGet("schedules/{id:int}/attendance")]
    public async Task<IActionResult> GetAttendance(int id, [FromQuery] DateTime date)
        => Ok(ApiResponse<List<TrainingAttendanceRowDto>>.SuccessResponse(await _service.GetAttendanceAsync(id, date)));

    [HttpPost("schedules/{id:int}/attendance")]
    public async Task<IActionResult> MarkAttendance(int id, [FromBody] TrainingAttendanceMarkDto dto)
        => Ok(ApiResponse<List<TrainingAttendanceRowDto>>.SuccessResponse(await _service.MarkAttendanceAsync(id, dto), "Attendance saved."));

    // ---------------- Evaluation / history ----------------

    [HttpPut("participants/{participantId:int}/evaluation")]
    public async Task<IActionResult> SaveEvaluation(int participantId, [FromBody] TrainingEvaluationSaveDto dto)
        => Ok(ApiResponse<TrainingParticipantResponseDto>.SuccessResponse(await _service.SaveEvaluationAsync(participantId, dto), "Evaluation saved."));

    [HttpGet("employee/{employeeId:int}/history")]
    public async Task<IActionResult> History(int employeeId)
        => Ok(ApiResponse<List<TrainingHistoryDto>>.SuccessResponse(await _service.GetEmployeeHistoryAsync(employeeId)));
}

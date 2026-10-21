using HRMS.Application.DTOs.Training;

namespace HRMS.Application.Interfaces.Services;

public interface ITrainingService
{
    Task<List<TrainingScheduleResponseDto>> GetSchedulesAsync(string? status, int? programId, DateTime? fromDate, DateTime? toDate);
    Task<TrainingScheduleResponseDto> GetScheduleByIdAsync(int id);
    Task<TrainingScheduleResponseDto> CreateScheduleAsync(TrainingScheduleCreateDto dto);
    Task<TrainingScheduleResponseDto> UpdateScheduleAsync(int id, TrainingScheduleUpdateDto dto);
    Task<TrainingScheduleResponseDto> StartAsync(int id);
    Task<TrainingScheduleResponseDto> CompleteAsync(int id);
    Task<TrainingScheduleResponseDto> CancelAsync(int id);

    Task<List<TrainingParticipantResponseDto>> GetParticipantsAsync(int scheduleId);
    Task<List<TrainingParticipantResponseDto>> EnrollAsync(int scheduleId, TrainingEnrollDto dto);
    Task<List<TrainingParticipantResponseDto>> DropParticipantAsync(int participantId);

    Task<List<TrainingAttendanceRowDto>> GetAttendanceAsync(int scheduleId, DateTime date);
    Task<List<TrainingAttendanceRowDto>> MarkAttendanceAsync(int scheduleId, TrainingAttendanceMarkDto dto);

    Task<TrainingParticipantResponseDto> SaveEvaluationAsync(int participantId, TrainingEvaluationSaveDto dto);
    Task<List<TrainingHistoryDto>> GetEmployeeHistoryAsync(int employeeId);
}

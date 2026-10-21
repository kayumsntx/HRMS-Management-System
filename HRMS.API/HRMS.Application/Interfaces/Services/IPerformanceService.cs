using HRMS.Application.DTOs.Performance;

namespace HRMS.Application.Interfaces.Services;

public interface IPerformanceService
{
    Task<List<GoalResponseDto>> GetGoalsAsync(int? employeeId, int? periodId, string? status);
    Task<GoalResponseDto> CreateGoalAsync(GoalCreateDto dto);
    Task<GoalResponseDto> UpdateGoalAsync(int id, GoalUpdateDto dto);
    Task<GoalResponseDto> UpdateGoalProgressAsync(int id, GoalProgressDto dto, int? ownerEmployeeId);
    Task DeleteGoalAsync(int id);

    Task<List<EvaluationResponseDto>> GetEvaluationsAsync(int? employeeId, int? periodId, string? status);
    Task<EvaluationResponseDto> GetEvaluationByIdAsync(int id);
    Task<List<EvaluationResponseDto>> GetHistoryAsync(int employeeId, bool approvedOnly);
    Task<EvaluationResponseDto> CreateEvaluationAsync(EvaluationCreateDto dto);
    Task<EvaluationResponseDto> UpdateEvaluationAsync(int id, EvaluationUpdateDto dto);
    Task<EvaluationResponseDto> SubmitAsync(int id);
    Task<EvaluationResponseDto> ApproveAsync(int id);
    Task<EvaluationResponseDto> ReopenAsync(int id);
    Task DeleteEvaluationAsync(int id);
}

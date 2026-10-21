using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Performance;
using HRMS.Application.DTOs.Team;

namespace HRMS.Application.Interfaces.Services;

public interface ITeamService
{
    Task<List<TeamMemberDto>> GetMembersAsync(int managerEmployeeId);
    Task<List<AttendanceSummaryDto>> GetAttendanceSummaryAsync(int managerEmployeeId, int year, int month);
    Task<List<GoalResponseDto>> GetGoalsAsync(int managerEmployeeId, int? periodId, string? status);
    Task<GoalResponseDto> CreateGoalAsync(int managerEmployeeId, GoalCreateDto dto);
    Task<List<EvaluationResponseDto>> GetEvaluationsAsync(int managerEmployeeId, int? periodId, string? status);
    Task<EvaluationResponseDto> CreateEvaluationAsync(int managerEmployeeId, EvaluationCreateDto dto);
    Task<EvaluationResponseDto> UpdateEvaluationAsync(int managerEmployeeId, int evaluationId, EvaluationUpdateDto dto);
    Task<EvaluationResponseDto> SubmitEvaluationAsync(int managerEmployeeId, int evaluationId);
}

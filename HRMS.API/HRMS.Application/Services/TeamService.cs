using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Performance;
using HRMS.Application.DTOs.Team;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

// A manager works only with employees whose ReportingManager is that manager.
// Approving an evaluation stays with Admin/HR (/api/performance/evaluations/{id}/approve).
public class TeamService : ITeamService
{
    private readonly IUnitOfWork _uow;
    private readonly IPerformanceService _performance;
    private readonly IAttendanceService _attendance;

    public TeamService(IUnitOfWork uow, IPerformanceService performance, IAttendanceService attendance)
    {
        _uow = uow;
        _performance = performance;
        _attendance = attendance;
    }

    private async Task<List<Employee>> GetTeamAsync(int managerEmployeeId)
        => (await _uow.Repository<Employee>().FindAsync(e => e.IsActive && e.ReportingManagerId == managerEmployeeId))
            .OrderBy(e => e.EmployeeCode.Length).ThenBy(e => e.EmployeeCode).ToList();

    private async Task EnsureTeamMemberAsync(int managerEmployeeId, int employeeId)
    {
        var isMember = await _uow.Repository<Employee>()
            .AnyAsync(e => e.Id == employeeId && e.IsActive && e.ReportingManagerId == managerEmployeeId);

        // Reported as "not found" on purpose so other people's data is not revealed.
        if (!isMember)
            throw new NotFoundException(nameof(Employee), employeeId);
    }

    public async Task<List<TeamMemberDto>> GetMembersAsync(int managerEmployeeId)
    {
        var team = await GetTeamAsync(managerEmployeeId);
        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);

        return team.Select(e => new TeamMemberDto
        {
            EmployeeId = e.Id,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            DesignationName = designations.GetValueOrDefault(e.DesignationId),
            DepartmentName = departments.GetValueOrDefault(e.DepartmentId),
            MobileNumber = e.MobileNumber
        }).ToList();
    }

    public async Task<List<AttendanceSummaryDto>> GetAttendanceSummaryAsync(int managerEmployeeId, int year, int month)
    {
        var result = new List<AttendanceSummaryDto>();
        foreach (var member in await GetTeamAsync(managerEmployeeId))
            result.Add(await _attendance.GetMonthlySummaryAsync(member.Id, year, month));

        return result;
    }

    public async Task<List<GoalResponseDto>> GetGoalsAsync(int managerEmployeeId, int? periodId, string? status)
    {
        var result = new List<GoalResponseDto>();
        foreach (var member in await GetTeamAsync(managerEmployeeId))
            result.AddRange(await _performance.GetGoalsAsync(member.Id, periodId, status));

        return result;
    }

    public async Task<GoalResponseDto> CreateGoalAsync(int managerEmployeeId, GoalCreateDto dto)
    {
        await EnsureTeamMemberAsync(managerEmployeeId, dto.EmployeeId);
        return await _performance.CreateGoalAsync(dto);
    }

    public async Task<List<EvaluationResponseDto>> GetEvaluationsAsync(int managerEmployeeId, int? periodId, string? status)
    {
        var result = new List<EvaluationResponseDto>();
        foreach (var member in await GetTeamAsync(managerEmployeeId))
            result.AddRange(await _performance.GetEvaluationsAsync(member.Id, periodId, status));

        return result;
    }

    // The evaluator is the logged-in manager (taken from the token inside PerformanceService).
    public async Task<EvaluationResponseDto> CreateEvaluationAsync(int managerEmployeeId, EvaluationCreateDto dto)
    {
        await EnsureTeamMemberAsync(managerEmployeeId, dto.EmployeeId);
        return await _performance.CreateEvaluationAsync(dto);
    }

    public async Task<EvaluationResponseDto> UpdateEvaluationAsync(int managerEmployeeId, int evaluationId, EvaluationUpdateDto dto)
    {
        await EnsureOwnEvaluationAsync(managerEmployeeId, evaluationId);
        return await _performance.UpdateEvaluationAsync(evaluationId, dto);
    }

    public async Task<EvaluationResponseDto> SubmitEvaluationAsync(int managerEmployeeId, int evaluationId)
    {
        await EnsureOwnEvaluationAsync(managerEmployeeId, evaluationId);
        return await _performance.SubmitAsync(evaluationId);
    }

    private async Task EnsureOwnEvaluationAsync(int managerEmployeeId, int evaluationId)
    {
        var evaluation = await _performance.GetEvaluationByIdAsync(evaluationId);

        if (evaluation.EvaluatorEmployeeId != managerEmployeeId)
            throw new NotFoundException(nameof(PerformanceEvaluation), evaluationId);

        await EnsureTeamMemberAsync(managerEmployeeId, evaluation.EmployeeId);
    }
}

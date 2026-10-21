using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Performance;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class PerformanceService : IPerformanceService
{
    private static readonly string GOpen = GoalStatus.Open.ToString();
    private static readonly string GInProgress = GoalStatus.InProgress.ToString();
    private static readonly string GCompleted = GoalStatus.Completed.ToString();
    private static readonly string GCancelled = GoalStatus.Cancelled.ToString();
    private static readonly string EDraft = EvaluationStatus.Draft.ToString();
    private static readonly string ESubmitted = EvaluationStatus.Submitted.ToString();
    private static readonly string EApproved = EvaluationStatus.Approved.ToString();

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _notifier;

    public PerformanceService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser, INotificationPublisher notifier)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    // =====================================================================
    // GOALS
    // =====================================================================
    public async Task<List<GoalResponseDto>> GetGoalsAsync(int? employeeId, int? periodId, string? status)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<GoalStatus>(status, "status");

        var items = await _uow.Repository<EmployeeGoal>().FindAsync(g =>
            (!employeeId.HasValue || g.EmployeeId == employeeId.Value) &&
            (!periodId.HasValue || g.PerformancePeriodId == periodId.Value) &&
            (statusText == null || g.Status == statusText));

        return await MapGoalsAsync(items.OrderByDescending(g => g.Id).Take(500));
    }

    public async Task<GoalResponseDto> CreateGoalAsync(GoalCreateDto dto)
    {
        await EnsureActiveEmployeeAsync(dto.EmployeeId);
        await EnsureActivePeriodAsync(dto.PerformancePeriodId);

        var goal = new EmployeeGoal
        {
            EmployeeId = dto.EmployeeId,
            PerformancePeriodId = dto.PerformancePeriodId,
            GoalTitle = dto.GoalTitle.Trim(),
            Description = dto.Description?.Trim(),
            TargetValue = dto.TargetValue,
            Status = GOpen
        };

        await _uow.Repository<EmployeeGoal>().AddAsync(goal);
        await _uow.SaveChangesAsync();
        return (await MapGoalsAsync(new[] { goal })).First();
    }

    public async Task<GoalResponseDto> UpdateGoalAsync(int id, GoalUpdateDto dto)
    {
        var goal = await LoadGoalAsync(id);

        goal.GoalTitle = dto.GoalTitle.Trim();
        goal.Description = dto.Description?.Trim();
        goal.TargetValue = dto.TargetValue;
        goal.AchievedValue = dto.AchievedValue;
        goal.Status = EnumHelper.Normalize<GoalStatus>(dto.Status, "status");
        goal.UpdatedDate = DateTime.UtcNow;

        _uow.Repository<EmployeeGoal>().Update(goal);
        await _uow.SaveChangesAsync();
        return (await MapGoalsAsync(new[] { goal })).First();
    }

    // Employees update only the achieved value; the status follows automatically.
    public async Task<GoalResponseDto> UpdateGoalProgressAsync(int id, GoalProgressDto dto, int? ownerEmployeeId)
    {
        var goal = await LoadGoalAsync(id);

        if (ownerEmployeeId.HasValue && goal.EmployeeId != ownerEmployeeId.Value)
            throw new NotFoundException(nameof(EmployeeGoal), id);

        if (goal.Status == GCompleted || goal.Status == GCancelled)
            throw new BadRequestException($"A {goal.Status} goal cannot be updated.");

        goal.AchievedValue = dto.AchievedValue;
        goal.Status = goal.TargetValue.HasValue && dto.AchievedValue >= goal.TargetValue.Value ? GCompleted
                    : dto.AchievedValue > 0 ? GInProgress
                    : GOpen;
        goal.UpdatedDate = DateTime.UtcNow;

        _uow.Repository<EmployeeGoal>().Update(goal);
        await _uow.SaveChangesAsync();
        return (await MapGoalsAsync(new[] { goal })).First();
    }

    public async Task DeleteGoalAsync(int id)
    {
        var goal = await LoadGoalAsync(id);
        goal.Status = GCancelled;
        goal.UpdatedDate = DateTime.UtcNow;

        _uow.Repository<EmployeeGoal>().Update(goal);
        await _uow.SaveChangesAsync();
    }

    // =====================================================================
    // EVALUATIONS
    // =====================================================================
    public async Task<List<EvaluationResponseDto>> GetEvaluationsAsync(int? employeeId, int? periodId, string? status)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<EvaluationStatus>(status, "status");

        var items = await _uow.Repository<PerformanceEvaluation>().FindAsync(e =>
            (!employeeId.HasValue || e.EmployeeId == employeeId.Value) &&
            (!periodId.HasValue || e.PerformancePeriodId == periodId.Value) &&
            (statusText == null || e.Status == statusText));

        return await MapEvaluationsAsync(items.OrderByDescending(e => e.Id).Take(500));
    }

    public async Task<EvaluationResponseDto> GetEvaluationByIdAsync(int id)
        => (await MapEvaluationsAsync(new[] { await LoadEvaluationAsync(id) })).First();

    public async Task<List<EvaluationResponseDto>> GetHistoryAsync(int employeeId, bool approvedOnly)
    {
        if (await _uow.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var items = await _uow.Repository<PerformanceEvaluation>()
            .FindAsync(e => e.EmployeeId == employeeId && (!approvedOnly || e.Status == EApproved));

        var result = await MapEvaluationsAsync(items);
        return result.OrderByDescending(r => r.PeriodStartDate).ToList();
    }

    public async Task<EvaluationResponseDto> CreateEvaluationAsync(EvaluationCreateDto dto)
    {
        var employee = await EnsureActiveEmployeeAsync(dto.EmployeeId);
        var period = await EnsureActivePeriodAsync(dto.PerformancePeriodId);

        if (period.StartDate.Date > DateTime.Now.Date)
            throw new BadRequestException("This performance period has not started yet.");

        if (await _uow.Repository<PerformanceEvaluation>().AnyAsync(e => e.EmployeeId == dto.EmployeeId && e.PerformancePeriodId == dto.PerformancePeriodId))
            throw new BadRequestException("An evaluation already exists for this employee and period.");

        var evaluatorId = _currentUser.EmployeeId;
        if (evaluatorId.HasValue && evaluatorId.Value == employee.Id)
            throw new BadRequestException("An employee cannot evaluate himself/herself.");

        var scores = dto.KpiScores ?? new List<KpiScoreDto>();
        var kpis = await ValidateScoresAsync(employee, scores);

        var evaluation = new PerformanceEvaluation
        {
            EmployeeId = employee.Id,
            PerformancePeriodId = period.Id,
            EvaluatorEmployeeId = evaluatorId,
            Comments = dto.Comments?.Trim(),
            Status = EDraft,
            KpiEvaluations = scores.Select(s => new KpiEvaluation { KpiId = s.KpiId, Score = s.Score, Comments = s.Comments?.Trim() }).ToList()
        };

        ApplyTotals(evaluation, evaluation.KpiEvaluations, kpis);

        await _uow.Repository<PerformanceEvaluation>().AddAsync(evaluation);
        await _uow.SaveChangesAsync();
        return await GetEvaluationByIdAsync(evaluation.Id);
    }

    public async Task<EvaluationResponseDto> UpdateEvaluationAsync(int id, EvaluationUpdateDto dto)
    {
        var evaluation = await LoadEvaluationAsync(id);
        if (evaluation.Status != EDraft)
            throw new BadRequestException($"Only a Draft evaluation can be edited. Current status: {evaluation.Status}.");

        var employee = await _uow.Repository<Employee>().GetByIdAsync(evaluation.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), evaluation.EmployeeId);

        var scores = dto.KpiScores ?? new List<KpiScoreDto>();
        var kpis = await ValidateScoresAsync(employee, scores);

        // Update in place / add / remove (avoids unique index conflicts on (evaluation, KPI)).
        var existing = await _uow.Repository<KpiEvaluation>().FindAsync(k => k.PerformanceEvaluationId == id);
        var incoming = scores.ToDictionary(s => s.KpiId);
        var kept = new List<KpiEvaluation>();

        foreach (var row in existing)
        {
            if (incoming.TryGetValue(row.KpiId, out var s))
            {
                row.Score = s.Score;
                row.Comments = s.Comments?.Trim();
                row.UpdatedDate = DateTime.UtcNow;
                _uow.Repository<KpiEvaluation>().Update(row);
                kept.Add(row);
            }
            else
            {
                _uow.Repository<KpiEvaluation>().Delete(row);
            }
        }

        var existingKpiIds = existing.Select(r => r.KpiId).ToHashSet();
        var added = scores.Where(s => !existingKpiIds.Contains(s.KpiId))
            .Select(s => new KpiEvaluation { PerformanceEvaluationId = id, KpiId = s.KpiId, Score = s.Score, Comments = s.Comments?.Trim() })
            .ToList();

        if (added.Count > 0)
            await _uow.Repository<KpiEvaluation>().AddRangeAsync(added);

        evaluation.Comments = dto.Comments?.Trim();
        evaluation.UpdatedDate = DateTime.UtcNow;
        ApplyTotals(evaluation, kept.Concat(added).ToList(), kpis);
        _uow.Repository<PerformanceEvaluation>().Update(evaluation);

        await _uow.SaveChangesAsync();
        return await GetEvaluationByIdAsync(id);
    }

    public async Task<EvaluationResponseDto> SubmitAsync(int id)
    {
        var evaluation = await LoadEvaluationAsync(id);
        if (evaluation.Status != EDraft)
            throw new BadRequestException($"Only a Draft evaluation can be submitted. Current status: {evaluation.Status}.");

        if (!await _uow.Repository<KpiEvaluation>().AnyAsync(k => k.PerformanceEvaluationId == id))
            throw new BadRequestException("At least one KPI score is required before submitting.");

        evaluation.Status = ESubmitted;
        evaluation.EvaluatedDate = DateTime.UtcNow;
        evaluation.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<PerformanceEvaluation>().Update(evaluation);

        await _uow.SaveChangesAsync();
        return await GetEvaluationByIdAsync(id);
    }

    public async Task<EvaluationResponseDto> ApproveAsync(int id)
    {
        var evaluation = await LoadEvaluationAsync(id);
        if (evaluation.Status != ESubmitted)
            throw new BadRequestException($"Only a Submitted evaluation can be approved. Current status: {evaluation.Status}.");

        evaluation.Status = EApproved;
        evaluation.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<PerformanceEvaluation>().Update(evaluation);

        await _notifier.NotifyEmployeeAsync(evaluation.EmployeeId, NotificationCategory.General, "Performance evaluation published",
            "Your performance evaluation has been approved. You can view it from your profile.", "/my/evaluations");

        await _uow.SaveChangesAsync();
        return await GetEvaluationByIdAsync(id);
    }

    public async Task<EvaluationResponseDto> ReopenAsync(int id)
    {
        var evaluation = await LoadEvaluationAsync(id);
        if (evaluation.Status != ESubmitted)
            throw new BadRequestException($"Only a Submitted evaluation can be re-opened. Current status: {evaluation.Status}.");

        evaluation.Status = EDraft;
        evaluation.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<PerformanceEvaluation>().Update(evaluation);

        await _uow.SaveChangesAsync();
        return await GetEvaluationByIdAsync(id);
    }

    public async Task DeleteEvaluationAsync(int id)
    {
        var evaluation = await LoadEvaluationAsync(id);
        if (evaluation.Status != EDraft)
            throw new BadRequestException($"Only a Draft evaluation can be deleted. Current status: {evaluation.Status}.");

        // KPI scores are removed by the database (cascade).
        _uow.Repository<PerformanceEvaluation>().Delete(evaluation);
        await _uow.SaveChangesAsync();
    }

    // ---------------- Private Helpers ----------------

    private static string RatingFor(decimal score)
        => score >= 90 ? "Outstanding"
         : score >= 75 ? "Exceeds Expectations"
         : score >= 60 ? "Meets Expectations"
         : score >= 40 ? "Needs Improvement"
         : "Unsatisfactory";

    private static void ApplyTotals(PerformanceEvaluation evaluation, List<KpiEvaluation> scores, Dictionary<int, Kpi> kpis)
    {
        if (scores.Count == 0)
        {
            evaluation.TotalScore = null;
            evaluation.Rating = null;
            return;
        }

        var weightSum = scores.Sum(s => kpis[s.KpiId].Weight);
        var total = weightSum > 0
            ? scores.Sum(s => s.Score * kpis[s.KpiId].Weight) / weightSum
            : scores.Average(s => s.Score);

        evaluation.TotalScore = Math.Round(total, 2);
        evaluation.Rating = RatingFor(total);
    }

    // Every KPI must exist, be active and apply to the employee's department/designation.
    private async Task<Dictionary<int, Kpi>> ValidateScoresAsync(Employee employee, List<KpiScoreDto> scores)
    {
        var ids = scores.Select(s => s.KpiId).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, Kpi>();

        var kpis = (await _uow.Repository<Kpi>().FindAsync(k => ids.Contains(k.Id))).ToDictionary(k => k.Id);

        foreach (var id in ids)
        {
            if (!kpis.TryGetValue(id, out var kpi))
                throw new NotFoundException(nameof(Kpi), id);

            if (!kpi.IsActive)
                throw new BadRequestException($"KPI '{kpi.KpiName}' is inactive.");

            if ((kpi.DepartmentId.HasValue && kpi.DepartmentId != employee.DepartmentId) ||
                (kpi.DesignationId.HasValue && kpi.DesignationId != employee.DesignationId))
                throw new BadRequestException($"KPI '{kpi.KpiName}' does not apply to this employee's department/designation.");
        }

        return kpis;
    }

    private async Task<Employee> EnsureActiveEmployeeAsync(int id)
    {
        var employee = await _uow.Repository<Employee>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(Employee), id);
        if (!employee.IsActive)
            throw new BadRequestException("This employee is inactive.");
        return employee;
    }

    private async Task<PerformancePeriod> EnsureActivePeriodAsync(int id)
    {
        var period = await _uow.Repository<PerformancePeriod>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(PerformancePeriod), id);
        if (!period.IsActive)
            throw new BadRequestException("This performance period is inactive.");
        return period;
    }

    private async Task<EmployeeGoal> LoadGoalAsync(int id)
        => await _uow.Repository<EmployeeGoal>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(EmployeeGoal), id);

    private async Task<PerformanceEvaluation> LoadEvaluationAsync(int id)
        => await _uow.Repository<PerformanceEvaluation>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(PerformanceEvaluation), id);

    private async Task<List<GoalResponseDto>> MapGoalsAsync(IEnumerable<EmployeeGoal> items)
    {
        var list = items.ToList();
        var employees = (await _uow.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id);
        var periods = (await _uow.Repository<PerformancePeriod>().GetAllAsync()).ToDictionary(p => p.Id, p => p.PeriodName);

        var result = _mapper.Map<List<GoalResponseDto>>(list);
        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }

            dto.PeriodName = periods.GetValueOrDefault(dto.PerformancePeriodId);

            if (dto.TargetValue.HasValue && dto.TargetValue.Value > 0)
                dto.ProgressPercent = Math.Round((dto.AchievedValue ?? 0m) / dto.TargetValue.Value * 100m, 1);
        }

        return result;
    }

    private async Task<List<EvaluationResponseDto>> MapEvaluationsAsync(IEnumerable<PerformanceEvaluation> items)
    {
        var list = items.ToList();
        var employees = (await _uow.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id);
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);
        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);
        var periods = (await _uow.Repository<PerformancePeriod>().GetAllAsync()).ToDictionary(p => p.Id);
        var kpis = (await _uow.Repository<Kpi>().GetAllAsync()).ToDictionary(k => k.Id);

        var ids = list.Select(e => e.Id).ToList();
        var scores = ids.Count == 0 ? new List<KpiEvaluation>() : await _uow.Repository<KpiEvaluation>().FindAsync(k => ids.Contains(k.PerformanceEvaluationId));

        var result = _mapper.Map<List<EvaluationResponseDto>>(list);
        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
                dto.DepartmentName = departments.GetValueOrDefault(employee.DepartmentId);
                dto.DesignationName = designations.GetValueOrDefault(employee.DesignationId);
            }

            if (dto.EvaluatorEmployeeId.HasValue && employees.TryGetValue(dto.EvaluatorEmployeeId.Value, out var evaluator))
                dto.EvaluatorName = evaluator.FullName;

            if (periods.TryGetValue(dto.PerformancePeriodId, out var period))
            {
                dto.PeriodName = period.PeriodName;
                dto.PeriodStartDate = period.StartDate;
                dto.PeriodEndDate = period.EndDate;
            }

            dto.KpiScores = scores.Where(s => s.PerformanceEvaluationId == dto.Id).OrderBy(s => s.Id)
                .Select(s => new KpiScoreResponseDto
                {
                    Id = s.Id,
                    KpiId = s.KpiId,
                    KpiName = kpis.GetValueOrDefault(s.KpiId)?.KpiName,
                    Weight = kpis.GetValueOrDefault(s.KpiId)?.Weight ?? 0m,
                    Score = s.Score,
                    Comments = s.Comments
                }).ToList();
        }

        return result;
    }
}

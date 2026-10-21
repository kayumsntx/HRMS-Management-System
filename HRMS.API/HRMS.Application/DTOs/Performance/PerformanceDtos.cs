namespace HRMS.Application.DTOs.Performance;

public class GoalCreateDto
{
    public int EmployeeId { get; set; }
    public int PerformancePeriodId { get; set; }
    public string GoalTitle { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? TargetValue { get; set; }
}

public class GoalUpdateDto
{
    public string GoalTitle { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AchievedValue { get; set; }
    public string Status { get; set; } = default!;   // Open, InProgress, Completed, Cancelled
}

public class GoalProgressDto
{
    public decimal AchievedValue { get; set; }
}

public class GoalResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public int PerformancePeriodId { get; set; }
    public string? PeriodName { get; set; }
    public string GoalTitle { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AchievedValue { get; set; }
    public decimal? ProgressPercent { get; set; }
    public string Status { get; set; } = default!;
}

public class KpiScoreDto
{
    public int KpiId { get; set; }
    public decimal Score { get; set; }   // 0 - 100
    public string? Comments { get; set; }
}

public class EvaluationCreateDto
{
    public int EmployeeId { get; set; }
    public int PerformancePeriodId { get; set; }
    public string? Comments { get; set; }
    public List<KpiScoreDto> KpiScores { get; set; } = new();
}

public class EvaluationUpdateDto
{
    public string? Comments { get; set; }
    public List<KpiScoreDto> KpiScores { get; set; } = new();
}

public class KpiScoreResponseDto
{
    public int Id { get; set; }
    public int KpiId { get; set; }
    public string? KpiName { get; set; }
    public decimal Weight { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
}

public class EvaluationResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public int PerformancePeriodId { get; set; }
    public string? PeriodName { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public int? EvaluatorEmployeeId { get; set; }
    public string? EvaluatorName { get; set; }
    public decimal? TotalScore { get; set; }
    public string? Rating { get; set; }
    public string? Comments { get; set; }
    public string Status { get; set; } = default!;
    public DateTime? EvaluatedDate { get; set; }
    public List<KpiScoreResponseDto> KpiScores { get; set; } = new();
}

using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class PerformancePeriod : BaseEntity
{
    public string PeriodName { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class Kpi : BaseEntity
{
    public string KpiName { get; set; } = default!;
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public decimal Weight { get; set; }
    public string? Description { get; set; }
}

public class EmployeeGoal : BaseEntity
{
    public int EmployeeId { get; set; }
    public int PerformancePeriodId { get; set; }
    public string GoalTitle { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AchievedValue { get; set; }
    public string Status { get; set; } = "Open";
}

public class PerformanceEvaluation : BaseEntity
{
    public int EmployeeId { get; set; }
    public int PerformancePeriodId { get; set; }
    public int? EvaluatorEmployeeId { get; set; }
    public decimal? TotalScore { get; set; }
    public string? Rating { get; set; }
    public string? Comments { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? EvaluatedDate { get; set; }
    public List<KpiEvaluation> KpiEvaluations { get; set; } = new();
}

public class KpiEvaluation : BaseEntity
{
    public int PerformanceEvaluationId { get; set; }
    public int KpiId { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
}

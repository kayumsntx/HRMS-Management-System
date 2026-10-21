using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class PerformancePeriodConfiguration : IEntityTypeConfiguration<PerformancePeriod>
{
    public void Configure(EntityTypeBuilder<PerformancePeriod> b)
    {
        b.ToTable("PerformancePeriods");
        b.Property(x => x.PeriodName).HasMaxLength(100).IsRequired();
        b.Property(x => x.StartDate).HasColumnType("date");
        b.Property(x => x.EndDate).HasColumnType("date");
        b.HasIndex(x => x.PeriodName).IsUnique();
    }
}

public class KpiConfiguration : IEntityTypeConfiguration<Kpi>
{
    public void Configure(EntityTypeBuilder<Kpi> b)
    {
        b.ToTable("Kpis");
        b.Property(x => x.KpiName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Weight).HasPrecision(5, 2);
        b.RestrictFk<Kpi, Department>(x => x.DepartmentId);
        b.RestrictFk<Kpi, Designation>(x => x.DesignationId);
    }
}

public class EmployeeGoalConfiguration : IEntityTypeConfiguration<EmployeeGoal>
{
    public void Configure(EntityTypeBuilder<EmployeeGoal> b)
    {
        b.ToTable("EmployeeGoals");
        b.Property(x => x.GoalTitle).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.TargetValue).HasPrecision(12, 2);
        b.Property(x => x.AchievedValue).HasPrecision(12, 2);
        b.HasIndex(x => new { x.EmployeeId, x.PerformancePeriodId });
        b.RestrictFk<EmployeeGoal, Employee>(x => x.EmployeeId);
        b.RestrictFk<EmployeeGoal, PerformancePeriod>(x => x.PerformancePeriodId);
    }
}

public class PerformanceEvaluationConfiguration : IEntityTypeConfiguration<PerformanceEvaluation>
{
    public void Configure(EntityTypeBuilder<PerformanceEvaluation> b)
    {
        b.ToTable("PerformanceEvaluations");
        b.Property(x => x.TotalScore).HasPrecision(5, 2);
        b.Property(x => x.Rating).HasMaxLength(30);
        b.Property(x => x.Comments).HasMaxLength(1000);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => new { x.EmployeeId, x.PerformancePeriodId }).IsUnique();
        b.RestrictFk<PerformanceEvaluation, Employee>(x => x.EmployeeId);
        b.RestrictFk<PerformanceEvaluation, PerformancePeriod>(x => x.PerformancePeriodId);
        b.RestrictFk<PerformanceEvaluation, Employee>(x => x.EvaluatorEmployeeId);
        b.HasMany(x => x.KpiEvaluations).WithOne().HasForeignKey(k => k.PerformanceEvaluationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class KpiEvaluationConfiguration : IEntityTypeConfiguration<KpiEvaluation>
{
    public void Configure(EntityTypeBuilder<KpiEvaluation> b)
    {
        b.ToTable("KpiEvaluations");
        b.Property(x => x.Score).HasPrecision(5, 2);
        b.Property(x => x.Comments).HasMaxLength(500);
        b.HasIndex(x => new { x.PerformanceEvaluationId, x.KpiId }).IsUnique();
        b.RestrictFk<KpiEvaluation, Kpi>(x => x.KpiId);
    }
}

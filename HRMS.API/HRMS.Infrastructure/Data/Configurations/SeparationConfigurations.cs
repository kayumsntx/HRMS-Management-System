using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EmployeeSeparationConfiguration : IEntityTypeConfiguration<EmployeeSeparation>
{
    public void Configure(EntityTypeBuilder<EmployeeSeparation> b)
    {
        b.ToTable("EmployeeSeparations");
        b.Property(x => x.SeparationType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.RequestDate).HasColumnType("date");
        b.Property(x => x.LastWorkingDate).HasColumnType("date");
        b.HasIndex(x => x.EmployeeId);
        // An employee can have only ONE open (Pending/Approved) separation.
        b.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("[Status] IN ('Pending','Approved')").HasDatabaseName("UX_EmployeeSeparations_Open");
        b.RestrictFk<EmployeeSeparation, Employee>(x => x.EmployeeId);
    }
}

public class ExitInterviewConfiguration : IEntityTypeConfiguration<ExitInterview>
{
    public void Configure(EntityTypeBuilder<ExitInterview> b)
    {
        b.ToTable("ExitInterviews");
        b.Property(x => x.Feedback).HasMaxLength(2000);
        b.Property(x => x.InterviewDate).HasColumnType("date");
        b.HasIndex(x => x.SeparationId).IsUnique();
        b.RestrictFk<ExitInterview, EmployeeSeparation>(x => x.SeparationId);
        b.RestrictFk<ExitInterview, Employee>(x => x.InterviewerEmployeeId);
    }
}

public class SeparationClearanceConfiguration : IEntityTypeConfiguration<SeparationClearance>
{
    public void Configure(EntityTypeBuilder<SeparationClearance> b)
    {
        b.ToTable("SeparationClearances");
        b.Property(x => x.DueAmount).HasPrecision(12, 2);
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => new { x.SeparationId, x.DepartmentId }).IsUnique();
        b.RestrictFk<SeparationClearance, EmployeeSeparation>(x => x.SeparationId);
        b.RestrictFk<SeparationClearance, Department>(x => x.DepartmentId);
    }
}

public class FinalSettlementConfiguration : IEntityTypeConfiguration<FinalSettlement>
{
    public void Configure(EntityTypeBuilder<FinalSettlement> b)
    {
        b.ToTable("FinalSettlements");
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.PayableSalary).HasPrecision(12, 2);
        b.Property(x => x.LeaveEncashment).HasPrecision(12, 2);
        b.Property(x => x.OtherPayable).HasPrecision(12, 2);
        b.Property(x => x.OtherDeduction).HasPrecision(12, 2);
        b.Property(x => x.TotalDeduction).HasPrecision(12, 2);
        b.Property(x => x.NetPayable).HasPrecision(12, 2);
        b.Property(x => x.SettlementDate).HasColumnType("date");
        b.HasIndex(x => x.SeparationId).IsUnique();
        b.RestrictFk<FinalSettlement, EmployeeSeparation>(x => x.SeparationId);
    }
}

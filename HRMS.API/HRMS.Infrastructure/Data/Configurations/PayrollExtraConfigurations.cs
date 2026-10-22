using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LeavePolicyConfiguration : IEntityTypeConfiguration<LeavePolicy>
{
    public void Configure(EntityTypeBuilder<LeavePolicy> b)
    {
        b.ToTable("LeavePolicies");
        b.Property(x => x.YearlyDays).HasPrecision(5, 2);
        b.Property(x => x.MaxCarryForwardDays).HasPrecision(5, 2);
        b.Property(x => x.EffectiveFrom).HasColumnType("date");
        b.Property(x => x.EffectiveTo).HasColumnType("date");
        b.HasIndex(x => new { x.LeaveTypeId, x.EffectiveFrom }).IsUnique();
        b.RestrictFk<LeavePolicy, LeaveType>(x => x.LeaveTypeId);
    }
}

public class BonusConfiguration : IEntityTypeConfiguration<Bonus>
{
    public void Configure(EntityTypeBuilder<Bonus> b)
    {
        b.ToTable("Bonuses");
        b.Property(x => x.BonusType).HasMaxLength(100).IsRequired();
        b.Property(x => x.Amount).HasPrecision(12, 2);
        b.Property(x => x.Remarks).HasMaxLength(300);
        b.HasIndex(x => new { x.BonusYear, x.BonusMonth });
        b.HasIndex(x => x.EmployeeId);
        b.RestrictFk<Bonus, Employee>(x => x.EmployeeId);
    }
}

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> b)
    {
        b.ToTable("Loans");
        b.Property(x => x.LoanAmount).HasPrecision(12, 2);
        b.Property(x => x.InstallmentAmount).HasPrecision(12, 2);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Purpose).HasMaxLength(300);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.StartDate).HasColumnType("date");
        b.HasIndex(x => x.EmployeeId);
        b.HasIndex(x => x.Status);
        // One Pending/Active loan per employee (enforced by the database as well).
        b.HasIndex(x => x.EmployeeId).IsUnique().HasFilter("[Status] IN ('Pending','Active')").HasDatabaseName("UX_Loans_Open");
        b.RestrictFk<Loan, Employee>(x => x.EmployeeId);
    }
}

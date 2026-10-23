using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class ShortLeaveConfiguration : IEntityTypeConfiguration<ShortLeave>
{
    public void Configure(EntityTypeBuilder<ShortLeave> b)
    {
        b.ToTable("ShortLeaves");
        b.Property(x => x.LeaveDate).HasColumnType("date");
        b.Property(x => x.TotalHours).HasPrecision(5, 2);
        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => new { x.EmployeeId, x.LeaveDate });
        b.HasIndex(x => x.Status);
        b.RestrictFk<ShortLeave, Employee>(x => x.EmployeeId);
        b.ToTable(t => t.HasCheckConstraint("CK_ShortLeave_Status", "[Status] IN ('Pending','Approved','Rejected','Cancelled')"));
    }
}

public class AttendanceMonthLockConfiguration : IEntityTypeConfiguration<AttendanceMonthLock>
{
    public void Configure(EntityTypeBuilder<AttendanceMonthLock> b)
    {
        b.ToTable("AttendanceMonthLocks");
        b.Property(x => x.Remarks).HasMaxLength(300);
        b.HasIndex(x => new { x.Year, x.Month }).IsUnique();
    }
}

public class BiometricSyncHistoryConfiguration : IEntityTypeConfiguration<BiometricSyncHistory>
{
    public void Configure(EntityTypeBuilder<BiometricSyncHistory> b)
    {
        b.ToTable("BiometricSyncHistories");
        b.Property(x => x.SyncType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.ErrorMessage).HasMaxLength(1000);
        b.HasIndex(x => new { x.BiometricDeviceId, x.StartTime });
        b.RestrictFk<BiometricSyncHistory, BiometricDevice>(x => x.BiometricDeviceId);
    }
}

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> b)
    {
        b.ToTable("SalaryComponents");
        b.Property(x => x.ComponentName).HasMaxLength(100).IsRequired();
        b.Property(x => x.ComponentType).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.ComponentName).IsUnique();
        b.ToTable(t => t.HasCheckConstraint("CK_SalaryComponent_Type", "[ComponentType] IN ('Allowance','Deduction')"));
    }
}

public class EmployeeSalaryComponentConfiguration : IEntityTypeConfiguration<EmployeeSalaryComponent>
{
    public void Configure(EntityTypeBuilder<EmployeeSalaryComponent> b)
    {
        b.ToTable("EmployeeSalaryComponents");
        b.Property(x => x.Amount).HasPrecision(12, 2);
        b.HasIndex(x => new { x.EmployeeSalaryId, x.SalaryComponentId }).IsUnique();
        b.RestrictFk<EmployeeSalaryComponent, EmployeeSalary>(x => x.EmployeeSalaryId);
        b.RestrictFk<EmployeeSalaryComponent, SalaryComponent>(x => x.SalaryComponentId);
    }
}

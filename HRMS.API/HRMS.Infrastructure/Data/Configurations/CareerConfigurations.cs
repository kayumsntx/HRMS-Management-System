using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EmployeeCareerHistoryConfiguration : IEntityTypeConfiguration<EmployeeCareerHistory>
{
    public void Configure(EntityTypeBuilder<EmployeeCareerHistory> b)
    {
        b.ToTable("EmployeeCareerHistories");
        b.Property(x => x.ChangeType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.EffectiveDate).HasColumnType("date");
        b.HasIndex(x => x.EmployeeId);
        b.HasIndex(x => new { x.Status, x.IsApplied });
        // Old/New ids are historical snapshots, so they intentionally have no foreign keys.
        b.RestrictFk<EmployeeCareerHistory, Employee>(x => x.EmployeeId);
    }
}

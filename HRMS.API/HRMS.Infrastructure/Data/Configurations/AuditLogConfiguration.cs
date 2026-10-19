using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.Property(x => x.UserName).HasMaxLength(100);
        builder.Property(x => x.ActionType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.TableName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RecordId).HasMaxLength(50);
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(x => x.NewValues).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.ActivityDate);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.TableName, x.RecordId });
    }
}

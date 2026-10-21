using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class RequisitionTypeConfiguration : IEntityTypeConfiguration<RequisitionType>
{
    public void Configure(EntityTypeBuilder<RequisitionType> b)
    {
        b.ToTable("RequisitionTypes");
        b.Property(x => x.TypeName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300);
        b.HasIndex(x => x.TypeName).IsUnique();
    }
}

public class RequisitionConfiguration : IEntityTypeConfiguration<Requisition>
{
    public void Configure(EntityTypeBuilder<Requisition> b)
    {
        b.ToTable("Requisitions");
        b.Property(x => x.RequisitionNo).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Purpose).HasMaxLength(500);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.RequisitionDate).HasColumnType("date");
        b.HasIndex(x => x.RequisitionNo).IsUnique();
        b.HasIndex(x => x.EmployeeId);
        b.HasIndex(x => x.Status);
        b.RestrictFk<Requisition, RequisitionType>(x => x.RequisitionTypeId);
        b.RestrictFk<Requisition, Employee>(x => x.EmployeeId);
        b.HasMany(x => x.Details).WithOne().HasForeignKey(d => d.RequisitionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RequisitionDetailConfiguration : IEntityTypeConfiguration<RequisitionDetail>
{
    public void Configure(EntityTypeBuilder<RequisitionDetail> b)
    {
        b.ToTable("RequisitionDetails");
        b.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(10, 2);
        b.Property(x => x.Unit).HasMaxLength(30);
        b.Property(x => x.Remarks).HasMaxLength(300);
    }
}

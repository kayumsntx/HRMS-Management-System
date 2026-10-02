using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EmployeeLeaveConfiguration : IEntityTypeConfiguration<EmployeeLeave>
{
    public void Configure(EntityTypeBuilder<EmployeeLeave> builder)
    {
        builder.ToTable("EmployeeLeaves");

        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
        builder.Property(x => x.NumberOfDays).HasPrecision(5, 2);

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LeaveType)
               .WithMany(x => x.EmployeeLeaves)
               .HasForeignKey(x => x.LeaveTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.Status);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_EmployeeLeave_Date", "[ToDate] >= [FromDate]"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_EmployeeLeave_Days", "[NumberOfDays] > 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_EmployeeLeave_Status",
            "[Status] IN ('Pending','Approved','Rejected','Cancelled')"));
    }
}
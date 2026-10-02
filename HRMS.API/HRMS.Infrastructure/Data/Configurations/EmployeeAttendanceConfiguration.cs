using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Data.Configurations
{
    public class EmployeeAttendanceConfiguration : IEntityTypeConfiguration<EmployeeAttendance>
    {
        public void Configure(EntityTypeBuilder<EmployeeAttendance> builder)
        {
            builder.ToTable("EmployeeAttendances");

            builder.Property(x => x.DayStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Remarks).HasMaxLength(500);

            builder.Property(x => x.WorkingHours).HasPrecision(5, 2);
            builder.Property(x => x.LateHours).HasPrecision(5, 2);
            builder.Property(x => x.EarlyOutHours).HasPrecision(5, 2);
            builder.Property(x => x.OTHours).HasPrecision(5, 2);
            builder.Property(x => x.TotalShortLeave).HasPrecision(5, 2);
            builder.Property(x => x.ActualShortLeave).HasPrecision(5, 2);

            builder.HasOne(x => x.Employee)
                   .WithMany()
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Shift)
                   .WithMany()
                   .HasForeignKey(x => x.ShiftId)
                   .OnDelete(DeleteBehavior.Restrict);

            // a employee give one  attendance record in a day
            builder.HasIndex(x => new { x.EmployeeId, x.WorkingDate }).IsUnique();

            builder.HasIndex(x => x.WorkingDate);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_EmployeeAttendance_DayStatus",
                "[DayStatus] IN ('Present','Absent','Late','Leave','Holiday','Weekend','HalfDay')"));
        }
    }
}

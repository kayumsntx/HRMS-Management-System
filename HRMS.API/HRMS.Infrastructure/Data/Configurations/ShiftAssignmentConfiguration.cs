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
    public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
    {
        public void Configure(EntityTypeBuilder<ShiftAssignment> builder)
        {
            builder.ToTable("ShiftAssignments");

            builder.HasOne(x => x.Employee)
                   .WithMany()
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Shift)
                   .WithMany(x => x.ShiftAssignments)
                   .HasForeignKey(x => x.ShiftId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.ShiftId);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_ShiftAssignment_Date",
                "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]"));
        }
    }
}

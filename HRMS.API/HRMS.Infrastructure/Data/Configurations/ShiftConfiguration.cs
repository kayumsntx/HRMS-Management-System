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
    public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
    {
        public void Configure(EntityTypeBuilder<Shift> builder)
        {
            builder.ToTable("Shifts");

            builder.Property(x => x.ShiftName).HasMaxLength(100).IsRequired();

            builder.HasIndex(x => x.ShiftName).IsUnique();

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Shift_GraceTime",
                "[GraceTimeMinutes] >= 0"));
        }
    }
}

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
    public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
    {
        public void Configure(EntityTypeBuilder<LeaveType> builder)
        {
            builder.ToTable("LeaveTypes");

            builder.Property(x => x.LeaveTypeName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(300);

            builder.HasIndex(x => x.LeaveTypeName).IsUnique();
        }
    }
}

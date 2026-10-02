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
    internal class EmployeeStatusConfiguration : IEntityTypeConfiguration<EmployeeStatus>
    {
        public void Configure(EntityTypeBuilder<EmployeeStatus> builder)
        {
            builder.ToTable("EmployeeStatuses");

            builder.Property(x => x.StatusName).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(300);

            builder.HasIndex(x => x.StatusName).IsUnique();
        }
    }
}

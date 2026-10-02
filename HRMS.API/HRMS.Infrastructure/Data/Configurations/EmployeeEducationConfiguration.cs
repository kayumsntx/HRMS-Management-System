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
    public class EmployeeEducationConfiguration : IEntityTypeConfiguration<EmployeeEducation>
    {
        public void Configure(EntityTypeBuilder<EmployeeEducation> builder)
        {
            builder.ToTable("EmployeeEducations");

            builder.Property(x => x.Degree).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Institution).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Board).HasMaxLength(100);
            builder.Property(x => x.Subject).HasMaxLength(150);
            builder.Property(x => x.Result).HasMaxLength(50);

            builder.HasOne(x => x.Employee)
                   .WithMany(x => x.Educations)
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade);   // if Employee Delete,  Education Delete

            builder.HasIndex(x => x.EmployeeId);
        }
    }
}

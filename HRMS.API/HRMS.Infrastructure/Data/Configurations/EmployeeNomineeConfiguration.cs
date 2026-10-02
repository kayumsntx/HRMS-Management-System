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
    public class EmployeeNomineeConfiguration : IEntityTypeConfiguration<EmployeeNominee>
    {
        public void Configure(EntityTypeBuilder<EmployeeNominee> builder)
        {
            builder.ToTable("EmployeeNominees");

            builder.Property(x => x.NomineeName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Relationship).HasMaxLength(50).IsRequired();
            builder.Property(x => x.MobileNumber).HasMaxLength(20);
            builder.Property(x => x.NationalIdNo).HasMaxLength(30);
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.SharePercentage).HasPrecision(5, 2);

            builder.HasOne(x => x.Employee)
                   .WithMany(x => x.Nominees)
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.EmployeeId);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_EmployeeNominee_SharePercentage",
                "[SharePercentage] IS NULL OR ([SharePercentage] >= 0 AND [SharePercentage] <= 100)"));
        }
    }
    
}

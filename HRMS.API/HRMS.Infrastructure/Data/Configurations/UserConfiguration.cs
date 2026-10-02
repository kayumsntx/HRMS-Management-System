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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(150);
            builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();

            builder.HasIndex(x => x.UserName).IsUnique();

            // Email filtered unique — if NULL , not check
            builder.HasIndex(x => x.Email)
                   .IsUnique()
                   .HasFilter("[Email] IS NOT NULL");

            builder.HasOne(x => x.Employee)
                   .WithMany()
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

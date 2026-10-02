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
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");

            // ---- Basic fields ----
            builder.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.NickName).HasMaxLength(100);
            builder.Property(x => x.Gender).HasMaxLength(20);
            builder.Property(x => x.BloodGroup).HasMaxLength(10);
            builder.Property(x => x.NationalIdNo).HasMaxLength(30);
            builder.Property(x => x.TinNumber).HasMaxLength(30);
            builder.Property(x => x.MobileNumber).HasMaxLength(20);
            builder.Property(x => x.FamilyMobileNumber).HasMaxLength(20);
            builder.Property(x => x.Email).HasMaxLength(150);
            builder.Property(x => x.FingerprintUserId).HasMaxLength(50);
            builder.Property(x => x.ProfileImage).HasMaxLength(500);

            // Decimal precision (SalaryBudget)
         

            // ---- Unique constraints ----
            builder.HasIndex(x => x.EmployeeCode).IsUnique();

            // Filtered unique index — NationalIdNo NULL  unique not check
            builder.HasIndex(x => x.NationalIdNo)
                   .IsUnique()
                   .HasFilter("[NationalIdNo] IS NOT NULL");

            // Filtered unique index — FingerprintUserId NULL unique not check
            builder.HasIndex(x => x.FingerprintUserId)
                   .IsUnique()
                   .HasFilter("[FingerprintUserId] IS NOT NULL");

            // ---- Foreign Keys ----
            builder.HasOne(x => x.EmployeeStatus)
                   .WithMany(x => x.Employees)
                   .HasForeignKey(x => x.EmployeeStatusId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Designation)
                   .WithMany(x => x.Employees)
                   .HasForeignKey(x => x.DesignationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Grade)
                   .WithMany(x => x.Employees)
                   .HasForeignKey(x => x.GradeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Company)
                   .WithMany(x => x.Employees)
                   .HasForeignKey(x => x.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Unit)
                   .WithMany(x => x.Employees)
                   .HasForeignKey(x => x.UnitId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                   .WithMany(x => x.Employees)
                   .HasForeignKey(x => x.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Section)
                   .WithMany(x => x.Employees)
                   .HasForeignKey(x => x.SectionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ---- Self-reference (Reporting Manager) ----
            builder.HasOne(x => x.ReportingManager)
                   .WithMany()
                   .HasForeignKey(x => x.ReportingManagerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

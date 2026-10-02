using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EmployeeSalaryConfiguration : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> builder)
    {
        builder.ToTable("EmployeeSalaries");

        builder.Property(x => x.BasicSalary).HasPrecision(12, 2);
        builder.Property(x => x.GrossSalary).HasPrecision(12, 2);
        builder.Property(x => x.OvertimeRatePerHour).HasPrecision(10, 2);

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EmployeeId);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_EmployeeSalary_Amount",
            "[BasicSalary] >= 0 AND [GrossSalary] >= 0"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_EmployeeSalary_Date",
            "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]"));
    }
}
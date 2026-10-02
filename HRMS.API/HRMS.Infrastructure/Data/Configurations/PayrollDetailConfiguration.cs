using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class PayrollDetailConfiguration : IEntityTypeConfiguration<PayrollDetail>
{
    public void Configure(EntityTypeBuilder<PayrollDetail> builder)
    {
        builder.ToTable("PayrollDetails");

        builder.Property(x => x.BasicSalary).HasPrecision(12, 2);
        builder.Property(x => x.TotalAllowance).HasPrecision(12, 2);
        builder.Property(x => x.OvertimeAmount).HasPrecision(12, 2);
        builder.Property(x => x.BonusAmount).HasPrecision(12, 2);
        builder.Property(x => x.TotalDeduction).HasPrecision(12, 2);
        builder.Property(x => x.GrossSalary).HasPrecision(12, 2);
        builder.Property(x => x.NetSalary).HasPrecision(12, 2);

        builder.HasOne(x => x.Payroll)
               .WithMany(x => x.PayrollDetails)
               .HasForeignKey(x => x.PayrollId)
               .OnDelete(DeleteBehavior.Cascade);   // Payroll delete Detail also delete

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PayrollId, x.EmployeeId }).IsUnique();
        builder.HasIndex(x => x.EmployeeId);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_PayrollDetail_Amounts",
            "[BasicSalary] >= 0 AND [TotalAllowance] >= 0 AND [OvertimeAmount] >= 0 " +
            "AND [BonusAmount] >= 0 AND [TotalDeduction] >= 0 AND [GrossSalary] >= 0 AND [NetSalary] >= 0"));
    }
}
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
        builder.ToTable("Payrolls");

        builder.Property(x => x.Status).HasMaxLength(30).IsRequired();

        builder.HasIndex(x => new { x.PayrollMonth, x.PayrollYear }).IsUnique();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Payroll_Month", "[PayrollMonth] BETWEEN 1 AND 12"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Payroll_Year", "[PayrollYear] BETWEEN 2000 AND 2100"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Payroll_Status",
            "[Status] IN ('Draft','Processed','Approved','Paid')"));
    }
}
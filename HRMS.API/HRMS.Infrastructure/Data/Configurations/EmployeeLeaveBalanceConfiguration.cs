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
    public class EmployeeLeaveBalanceConfiguration : IEntityTypeConfiguration<EmployeeLeaveBalance>
    {
        public void Configure(EntityTypeBuilder<EmployeeLeaveBalance> builder)
        {
            builder.ToTable("EmployeeLeaveBalances");

            builder.Property(x => x.OpeningBalance).HasPrecision(5, 2);
            builder.Property(x => x.UsedDays).HasPrecision(5, 2);

            // RemainingDays computed property — in AppDbContext.OnModelCreating already  Ignore 
    
            builder.Ignore(x => x.RemainingDays);

            builder.HasOne(x => x.Employee)
                   .WithMany()
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LeaveType)
                   .WithMany(x => x.LeaveBalances)
                   .HasForeignKey(x => x.LeaveTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.LeaveYear }).IsUnique();

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_EmployeeLeaveBalance_Year",
                "[LeaveYear] BETWEEN 2000 AND 2100"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_EmployeeLeaveBalance_Days",
                "[OpeningBalance] >= 0 AND [UsedDays] >= 0"));
        }
    }
}

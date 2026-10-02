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
    public class BiometricPunchConfiguration : IEntityTypeConfiguration<BiometricPunch>
    {
        public void Configure(EntityTypeBuilder<BiometricPunch> builder)
        {
            builder.ToTable("BiometricPunches");

            builder.Property(x => x.FingerprintUserId).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PunchType).HasMaxLength(20);
            builder.Property(x => x.VerificationType).HasMaxLength(30);
            builder.Property(x => x.DeviceTransactionId).HasMaxLength(100);

            builder.HasOne(x => x.BiometricDevice)
                   .WithMany(x => x.Punches)
                   .HasForeignKey(x => x.BiometricDeviceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                   .WithMany()
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            // sane device, same fingerprint user, same time — duplicate punch restrict
            builder.HasIndex(x => new { x.BiometricDeviceId, x.FingerprintUserId, x.PunchDateTime }).IsUnique();

            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.PunchDateTime);
        }
    }
}

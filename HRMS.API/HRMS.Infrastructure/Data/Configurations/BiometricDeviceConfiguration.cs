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
    public class BiometricDeviceConfiguration : IEntityTypeConfiguration<BiometricDevice>
    {
        public void Configure(EntityTypeBuilder<BiometricDevice> builder)
        {
            builder.ToTable("BiometricDevices");

            builder.Property(x => x.DeviceName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DeviceSerialNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DeviceIpAddress).HasMaxLength(50);
            builder.Property(x => x.LocationName).HasMaxLength(150);
            builder.Property(x => x.DeviceType).HasMaxLength(50);

            builder.HasIndex(x => x.DeviceSerialNumber).IsUnique();

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_BiometricDevice_Port",
                "[Port] IS NULL OR ([Port] BETWEEN 1 AND 65535)"));
        }
    }
}

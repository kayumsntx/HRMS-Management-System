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
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.Property(x => x.CompanyCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.Phone).HasMaxLength(20);
            builder.Property(x => x.Email).HasMaxLength(150);

            builder.HasIndex(x => x.CompanyCode).IsUnique();
            builder.HasIndex(x => x.CompanyName).IsUnique();
        }
    }
}

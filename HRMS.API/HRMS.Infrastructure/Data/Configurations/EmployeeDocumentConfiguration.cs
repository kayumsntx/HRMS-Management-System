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
    public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
    {
        public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
        {
            builder.ToTable("EmployeeDocuments");

            builder.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            builder.Property(x => x.FileExtension).HasMaxLength(20);
            builder.Property(x => x.Remarks).HasMaxLength(500);

            builder.HasOne(x => x.Employee)
                   .WithMany(x => x.Documents)
                   .HasForeignKey(x => x.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.EmployeeId);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_EmployeeDocument_DocumentType",
                "[DocumentType] IN ('CV','NID','Certificate','Other')"));
        }
    }
}

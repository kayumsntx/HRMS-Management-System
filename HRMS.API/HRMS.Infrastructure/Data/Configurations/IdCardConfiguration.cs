using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class IdCardConfiguration : IEntityTypeConfiguration<IdCard>
{
    public void Configure(EntityTypeBuilder<IdCard> builder)
    {
        builder.ToTable("IdCards");

        builder.Property(x => x.CardNo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IssueType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CardStatus).HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReIssueReason).HasMaxLength(300);
        builder.Property(x => x.QrCodeData).HasMaxLength(500);

        builder.HasOne(x => x.Employee)
               .WithMany()
               .HasForeignKey(x => x.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CardNo).IsUnique();

        // One employee can have only ONE Active card (enforced by the database).
        builder.HasIndex(x => x.EmployeeId)
               .IsUnique()
               .HasFilter("[CardStatus] = 'Active'")
               .HasDatabaseName("UX_IdCards_Employee_Active");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_IdCard_CardStatus", "[CardStatus] IN ('Active','Lost','Expired','Cancelled')"));

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_IdCard_IssueType", "[IssueType] IN ('Issue','ReIssue')"));
    }
}

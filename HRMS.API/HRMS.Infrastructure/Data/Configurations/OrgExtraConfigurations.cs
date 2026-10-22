using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class SubSectionConfiguration : IEntityTypeConfiguration<SubSection>
{
    public void Configure(EntityTypeBuilder<SubSection> b)
    {
        b.ToTable("SubSections");
        b.Property(x => x.SubSectionName).HasMaxLength(150).IsRequired();
        b.HasIndex(x => new { x.SectionId, x.SubSectionName }).IsUnique();
        b.RestrictFk<SubSection, Section>(x => x.SectionId);
    }
}

public class JobLocationConfiguration : IEntityTypeConfiguration<JobLocation>
{
    public void Configure(EntityTypeBuilder<JobLocation> b)
    {
        b.ToTable("JobLocations");
        b.Property(x => x.LocationName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500);
        b.HasIndex(x => x.LocationName).IsUnique();
    }
}

public class StaffCategoryConfiguration : IEntityTypeConfiguration<StaffCategory>
{
    public void Configure(EntityTypeBuilder<StaffCategory> b)
    {
        b.ToTable("StaffCategories");
        b.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.CategoryName).IsUnique();
    }
}

public class SkillCategoryConfiguration : IEntityTypeConfiguration<SkillCategory>
{
    public void Configure(EntityTypeBuilder<SkillCategory> b)
    {
        b.ToTable("SkillCategories");
        b.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.CategoryName).IsUnique();
    }
}

public class PaymentTypeConfiguration : IEntityTypeConfiguration<PaymentType>
{
    public void Configure(EntityTypeBuilder<PaymentType> b)
    {
        b.ToTable("PaymentTypes");
        b.Property(x => x.PaymentTypeName).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.PaymentTypeName).IsUnique();
    }
}

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> b)
    {
        b.ToTable("Banks");
        b.Property(x => x.BankName).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.BankName).IsUnique();
    }
}

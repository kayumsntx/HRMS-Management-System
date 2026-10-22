using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EmployeeSkillConfiguration : IEntityTypeConfiguration<EmployeeSkill>
{
    public void Configure(EntityTypeBuilder<EmployeeSkill> b)
    {
        b.ToTable("EmployeeSkills");
        b.Property(x => x.SkillName).HasMaxLength(150).IsRequired();
        b.Property(x => x.SkillLevel).HasMaxLength(30);
        b.Property(x => x.Remarks).HasMaxLength(300);
        b.HasIndex(x => x.EmployeeId);
        b.RestrictFk<EmployeeSkill, Employee>(x => x.EmployeeId);
        b.RestrictFk<EmployeeSkill, SkillCategory>(x => x.SkillCategoryId);
    }
}

public class EmployeePreviousJobConfiguration : IEntityTypeConfiguration<EmployeePreviousJob>
{
    public void Configure(EntityTypeBuilder<EmployeePreviousJob> b)
    {
        b.ToTable("EmployeePreviousJobs");
        b.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Designation).HasMaxLength(150);
        b.Property(x => x.LeavingReason).HasMaxLength(300);
        b.Property(x => x.LastSalary).HasPrecision(12, 2);
        b.Property(x => x.FromDate).HasColumnType("date");
        b.Property(x => x.ToDate).HasColumnType("date");
        b.HasIndex(x => x.EmployeeId);
        b.RestrictFk<EmployeePreviousJob, Employee>(x => x.EmployeeId);
    }
}

public class EmployeeAddressConfiguration : IEntityTypeConfiguration<EmployeeAddress>
{
    public void Configure(EntityTypeBuilder<EmployeeAddress> b)
    {
        b.ToTable("EmployeeAddresses");
        b.Property(x => x.AddressType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Village).HasMaxLength(150);
        b.Property(x => x.PostOffice).HasMaxLength(100);
        b.Property(x => x.PoliceStation).HasMaxLength(100);
        b.Property(x => x.District).HasMaxLength(100);
        b.Property(x => x.Division).HasMaxLength(100);
        b.Property(x => x.FullAddress).HasMaxLength(500);
        b.HasIndex(x => new { x.EmployeeId, x.AddressType }).IsUnique();
        b.RestrictFk<EmployeeAddress, Employee>(x => x.EmployeeId);
    }
}

public class EmployeeBankAccountConfiguration : IEntityTypeConfiguration<EmployeeBankAccount>
{
    public void Configure(EntityTypeBuilder<EmployeeBankAccount> b)
    {
        b.ToTable("EmployeeBankAccounts");
        b.Property(x => x.BranchName).HasMaxLength(150);
        b.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.EmployeeId);
        b.RestrictFk<EmployeeBankAccount, Employee>(x => x.EmployeeId);
        b.RestrictFk<EmployeeBankAccount, PaymentType>(x => x.PaymentTypeId);
        b.RestrictFk<EmployeeBankAccount, Bank>(x => x.BankId);
    }
}

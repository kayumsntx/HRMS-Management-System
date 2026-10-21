using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class ManpowerRequisitionConfiguration : IEntityTypeConfiguration<ManpowerRequisition>
{
    public void Configure(EntityTypeBuilder<ManpowerRequisition> b)
    {
        b.ToTable("ManpowerRequisitions");
        b.Property(x => x.RequisitionNo).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Justification).HasMaxLength(500);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.RequiredDate).HasColumnType("date");
        b.HasIndex(x => x.RequisitionNo).IsUnique();
        b.RestrictFk<ManpowerRequisition, Department>(x => x.DepartmentId);
        b.RestrictFk<ManpowerRequisition, Designation>(x => x.DesignationId);
    }
}

public class JobVacancyConfiguration : IEntityTypeConfiguration<JobVacancy>
{
    public void Configure(EntityTypeBuilder<JobVacancy> b)
    {
        b.ToTable("JobVacancies");
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.JobDescription).HasMaxLength(4000);
        b.Property(x => x.PostedDate).HasColumnType("date");
        b.Property(x => x.Deadline).HasColumnType("date");
        b.HasIndex(x => x.Status);
        b.RestrictFk<JobVacancy, ManpowerRequisition>(x => x.ManpowerRequisitionId);
        b.RestrictFk<JobVacancy, Department>(x => x.DepartmentId);
        b.RestrictFk<JobVacancy, Designation>(x => x.DesignationId);
    }
}

public class ApplicantConfiguration : IEntityTypeConfiguration<Applicant>
{
    public void Configure(EntityTypeBuilder<Applicant> b)
    {
        b.ToTable("Applicants");
        b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Email).HasMaxLength(150);
        b.Property(x => x.MobileNumber).HasMaxLength(20);
        b.Property(x => x.CvPath).HasMaxLength(500);
        b.Property(x => x.Source).HasMaxLength(100);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.ExpectedSalary).HasPrecision(12, 2);
        b.HasIndex(x => x.JobVacancyId);
        b.HasIndex(x => x.Status);
        b.RestrictFk<Applicant, JobVacancy>(x => x.JobVacancyId);
    }
}

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> b)
    {
        b.ToTable("Interviews");
        b.Property(x => x.Venue).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => new { x.ApplicantId, x.InterviewRound }).IsUnique();
        b.RestrictFk<Interview, Applicant>(x => x.ApplicantId);
    }
}

public class InterviewEvaluationConfiguration : IEntityTypeConfiguration<InterviewEvaluation>
{
    public void Configure(EntityTypeBuilder<InterviewEvaluation> b)
    {
        b.ToTable("InterviewEvaluations");
        b.Property(x => x.Score).HasPrecision(5, 2);
        b.Property(x => x.Comments).HasMaxLength(1000);
        b.Property(x => x.Result).HasMaxLength(10).IsRequired();
        b.HasOne<Interview>().WithMany().HasForeignKey(x => x.InterviewId).OnDelete(DeleteBehavior.Cascade);
        b.RestrictFk<InterviewEvaluation, Employee>(x => x.EvaluatorEmployeeId);
    }
}

public class JobOfferConfiguration : IEntityTypeConfiguration<JobOffer>
{
    public void Configure(EntityTypeBuilder<JobOffer> b)
    {
        b.ToTable("JobOffers");
        b.Property(x => x.OfferedSalary).HasPrecision(12, 2);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.OfferDate).HasColumnType("date");
        b.Property(x => x.JoiningDate).HasColumnType("date");
        b.HasIndex(x => x.ApplicantId);
        b.RestrictFk<JobOffer, Applicant>(x => x.ApplicantId);
        b.RestrictFk<JobOffer, Designation>(x => x.DesignationId);
        b.RestrictFk<JobOffer, Employee>(x => x.EmployeeId);
    }
}

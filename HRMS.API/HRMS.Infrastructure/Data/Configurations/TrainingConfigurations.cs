using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> b)
    {
        b.ToTable("TrainingPrograms");
        b.Property(x => x.ProgramName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.HasIndex(x => x.ProgramName).IsUnique();
    }
}

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> b)
    {
        b.ToTable("Trainers");
        b.Property(x => x.TrainerName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Organization).HasMaxLength(200);
        b.Property(x => x.Mobile).HasMaxLength(20);
        b.Property(x => x.Email).HasMaxLength(150);
        b.RestrictFk<Trainer, Employee>(x => x.EmployeeId);
    }
}

public class TrainingScheduleConfiguration : IEntityTypeConfiguration<TrainingSchedule>
{
    public void Configure(EntityTypeBuilder<TrainingSchedule> b)
    {
        b.ToTable("TrainingSchedules");
        b.Property(x => x.Venue).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Cost).HasPrecision(12, 2);
        b.HasIndex(x => x.Status);
        b.RestrictFk<TrainingSchedule, TrainingProgram>(x => x.TrainingProgramId);
        b.RestrictFk<TrainingSchedule, Trainer>(x => x.TrainerId);
    }
}

public class TrainingParticipantConfiguration : IEntityTypeConfiguration<TrainingParticipant>
{
    public void Configure(EntityTypeBuilder<TrainingParticipant> b)
    {
        b.ToTable("TrainingParticipants");
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.HasIndex(x => new { x.TrainingScheduleId, x.EmployeeId }).IsUnique();
        b.HasOne<TrainingSchedule>().WithMany().HasForeignKey(x => x.TrainingScheduleId).OnDelete(DeleteBehavior.Cascade);
        b.RestrictFk<TrainingParticipant, Employee>(x => x.EmployeeId);
    }
}

public class TrainingAttendanceConfiguration : IEntityTypeConfiguration<TrainingAttendance>
{
    public void Configure(EntityTypeBuilder<TrainingAttendance> b)
    {
        b.ToTable("TrainingAttendances");
        b.Property(x => x.AttendanceDate).HasColumnType("date");
        b.HasIndex(x => new { x.TrainingParticipantId, x.AttendanceDate }).IsUnique();
        b.HasOne<TrainingParticipant>().WithMany().HasForeignKey(x => x.TrainingParticipantId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TrainingEvaluationConfiguration : IEntityTypeConfiguration<TrainingEvaluation>
{
    public void Configure(EntityTypeBuilder<TrainingEvaluation> b)
    {
        b.ToTable("TrainingEvaluations");
        b.Property(x => x.Score).HasPrecision(5, 2);
        b.Property(x => x.Feedback).HasMaxLength(1000);
        b.HasIndex(x => x.TrainingParticipantId).IsUnique();
        b.HasOne<TrainingParticipant>().WithMany().HasForeignKey(x => x.TrainingParticipantId).OnDelete(DeleteBehavior.Cascade);
    }
}

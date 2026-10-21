using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class TrainingProgram : BaseEntity
{
    public string ProgramName { get; set; } = default!;
    public string? Description { get; set; }
    public int? DurationHours { get; set; }
}

public class Trainer : BaseEntity
{
    public string TrainerName { get; set; } = default!;
    public int? EmployeeId { get; set; }
    public string? Organization { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
}

public class TrainingSchedule : BaseEntity
{
    public int TrainingProgramId { get; set; }
    public int? TrainerId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Venue { get; set; }
    public decimal? Cost { get; set; }
    public string Status { get; set; } = "Planned";
}

public class TrainingParticipant : BaseEntity
{
    public int TrainingScheduleId { get; set; }
    public int EmployeeId { get; set; }
    public string Status { get; set; } = "Enrolled";
}

public class TrainingAttendance : BaseEntity
{
    public int TrainingParticipantId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public bool IsPresent { get; set; } = true;
}

public class TrainingEvaluation : BaseEntity
{
    public int TrainingParticipantId { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime EvaluatedDate { get; set; } = DateTime.UtcNow;
}

namespace HRMS.Application.DTOs.Training;

public class TrainingScheduleCreateDto
{
    public int TrainingProgramId { get; set; }
    public int? TrainerId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Venue { get; set; }
    public decimal? Cost { get; set; }
}

public class TrainingScheduleUpdateDto
{
    public int? TrainerId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Venue { get; set; }
    public decimal? Cost { get; set; }
}

public class TrainingScheduleResponseDto
{
    public int Id { get; set; }
    public int TrainingProgramId { get; set; }
    public string? ProgramName { get; set; }
    public int? TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Venue { get; set; }
    public decimal? Cost { get; set; }
    public string Status { get; set; } = default!;
    public int TotalDays { get; set; }
    public int ParticipantCount { get; set; }
}

public class TrainingEnrollDto
{
    public List<int> EmployeeIds { get; set; } = new();
}

public class TrainingParticipantResponseDto
{
    public int Id { get; set; }
    public int TrainingScheduleId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public string Status { get; set; } = default!;
    public int AttendedDays { get; set; }
    public int TotalDays { get; set; }
    public decimal AttendancePercent { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
}

public class TrainingAttendanceItemDto
{
    public int EmployeeId { get; set; }
    public bool IsPresent { get; set; }
}

public class TrainingAttendanceMarkDto
{
    public DateTime Date { get; set; }
    public List<TrainingAttendanceItemDto> Items { get; set; } = new();
}

public class TrainingAttendanceRowDto
{
    public int ParticipantId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public bool? IsPresent { get; set; }   // null = not marked yet
}

public class TrainingEvaluationSaveDto
{
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
}

public class TrainingHistoryDto
{
    public int ScheduleId { get; set; }
    public string? ProgramName { get; set; }
    public string? TrainerName { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? Venue { get; set; }
    public string ScheduleStatus { get; set; } = default!;
    public string ParticipantStatus { get; set; } = default!;
    public int AttendedDays { get; set; }
    public int TotalDays { get; set; }
    public decimal AttendancePercent { get; set; }
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
}

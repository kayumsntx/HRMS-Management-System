namespace HRMS.Application.DTOs.Shifts;

public class ShiftUpdateDto
{
    public string ShiftName { get; set; } = default!;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int GraceTimeMinutes { get; set; }
    public bool IsNightShift { get; set; }
    public bool IsActive { get; set; }
}

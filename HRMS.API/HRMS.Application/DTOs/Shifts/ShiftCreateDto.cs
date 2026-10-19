namespace HRMS.Application.DTOs.Shifts;

public class ShiftCreateDto
{
    public string ShiftName { get; set; } = default!;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int GraceTimeMinutes { get; set; }
    public bool IsNightShift { get; set; }
}

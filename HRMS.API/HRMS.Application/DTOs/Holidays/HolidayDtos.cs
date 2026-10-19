namespace HRMS.Application.DTOs.Holidays;

public class HolidayCreateDto
{
    public DateTime HolidayDate { get; set; }
    public string HolidayName { get; set; } = default!;
    public string? Description { get; set; }
}

public class HolidayUpdateDto
{
    public DateTime HolidayDate { get; set; }
    public string HolidayName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class HolidayResponseDto
{
    public int Id { get; set; }
    public DateTime HolidayDate { get; set; }
    public string DayName { get; set; } = default!;
    public string HolidayName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

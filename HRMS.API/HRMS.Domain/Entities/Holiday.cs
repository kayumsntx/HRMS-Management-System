using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class Holiday : BaseEntity
{
    public DateTime HolidayDate { get; set; }
    public string HolidayName { get; set; } = default!;
    public string? Description { get; set; }
}

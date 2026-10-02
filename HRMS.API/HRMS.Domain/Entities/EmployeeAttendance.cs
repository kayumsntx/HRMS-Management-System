using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class EmployeeAttendance:BaseEntity
    {
        public DateTime WorkingDate { get; set; }

        public DateTime? FirstInTime { get; set; }
        public DateTime? LastOutTime { get; set; }

        public decimal? WorkingHours { get; set; }
        public decimal? LateHours { get; set; }
        public decimal? EarlyOutHours { get; set; }
        public decimal? OTHours { get; set; }

        public string DayStatus { get; set; } = default!;   // Present, Absent, Late, Leave, Holiday, Weekend, HalfDay

        public decimal? TotalShortLeave { get; set; }
        public decimal? ActualShortLeave { get; set; }

        public string? Remarks { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        public int ShiftId { get; set; }
        public Shift Shift { get; set; } = default!;
    }
}

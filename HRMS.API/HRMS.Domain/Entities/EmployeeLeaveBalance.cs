using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class EmployeeLeaveBalance: BaseEntity
    {
        public int LeaveYear { get; set; }
        public decimal OpeningBalance { get; set; } = 0;
        public decimal UsedDays { get; set; } = 0;

        // EF Core  computed/read-only property 
        // Configuration  .HasComputedColumnSql()  generate 
        public decimal RemainingDays => OpeningBalance - UsedDays;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = default!;
    }
}

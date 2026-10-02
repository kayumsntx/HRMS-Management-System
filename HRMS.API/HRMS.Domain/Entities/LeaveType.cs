using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class LeaveType:BaseEntity
    {
        public string LeaveTypeName { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsPaid { get; set; } = true;

        // Navigation
        public ICollection<EmployeeLeaveBalance> LeaveBalances { get; set; } = new List<EmployeeLeaveBalance>();
        public ICollection<EmployeeLeave> EmployeeLeaves { get; set; } = new List<EmployeeLeave>();
    }
}

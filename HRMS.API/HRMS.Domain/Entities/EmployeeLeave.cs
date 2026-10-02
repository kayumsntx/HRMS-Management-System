using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class EmployeeLeave:BaseEntity
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public string? Reason { get; set; }

        public string Status { get; set; } = "Pending";   // Pending, Approved, Rejected, Cancelled

        public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RejectionReason { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        public int LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = default!;
    }
}

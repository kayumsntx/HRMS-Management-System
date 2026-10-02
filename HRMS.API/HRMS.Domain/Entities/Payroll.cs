using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class Payroll:BaseEntity
    {
        public int PayrollMonth { get; set; }
        public int PayrollYear { get; set; }
        public DateTime ProcessDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Draft";   // Draft, Processed, Approved, Paid
        public int? ProcessedBy { get; set; }

        // Navigation
        public ICollection<PayrollDetail> PayrollDetails { get; set; } = new List<PayrollDetail>();
    }
}

using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class EmployeeEducation:BaseEntity
    {
        public string Degree { get; set; } = default!;
        public string Institution { get; set; } = default!;
        public string? Board { get; set; }
        public string? Subject { get; set; }
        public int? PassingYear { get; set; }
        public string? Result { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
    }
}

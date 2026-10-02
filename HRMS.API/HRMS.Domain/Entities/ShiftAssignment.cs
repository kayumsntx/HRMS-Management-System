using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class ShiftAssignment:BaseEntity
    {
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;

        public int ShiftId { get; set; }
        public Shift Shift { get; set; } = default!;
    }
}

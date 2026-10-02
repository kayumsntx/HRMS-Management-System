using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class EmployeeSalary:BaseEntity
    {
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal? OvertimeRatePerHour { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
    }
}

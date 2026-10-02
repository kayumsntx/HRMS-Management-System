using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class PayrollDetail:BaseEntity
    {
        public decimal BasicSalary { get; set; }
        public decimal TotalAllowance { get; set; } = 0;
        public decimal OvertimeAmount { get; set; } = 0;
        public decimal BonusAmount { get; set; } = 0;
        public decimal TotalDeduction { get; set; } = 0;
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }

        public int PayrollId { get; set; }
        public Payroll Payroll { get; set; } = default!;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
    }
}

using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
   public class Unit:BaseEntity
    {
        public string UnitName { get; set; } = default!;
        public string? Address { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        // Navigation
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

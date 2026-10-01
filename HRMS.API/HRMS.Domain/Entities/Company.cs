using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class Company:BaseEntity
    {
        public string CompanyCode { get; set; } = default!;
        public string CompanyName { get; set; } = default!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        // Navigation
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

    }
}

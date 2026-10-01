using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class Section:BaseEntity
    {
        public string SectionName { get; set; } = default!;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = default!;

        // Navigation
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class Department:BaseEntity
    {
        public string DepartmentName { get; set; } = default!;

        // Navigation
        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

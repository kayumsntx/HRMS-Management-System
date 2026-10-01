using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class Designation:BaseEntity
    {
        public string DesignationName { get; set; } = default!;

        // Navigation
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

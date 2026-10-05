using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.EmployeeStatuses
{
    public class EmployeeStatusCreateDto
    {
        public string StatusName { get; set; } = default!;
        public string? Description { get; set; }
    }
}

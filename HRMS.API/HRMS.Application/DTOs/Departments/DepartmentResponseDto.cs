using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Departments
{
    public class DepartmentResponseDto
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

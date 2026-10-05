using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Employees
{
    public class EmployeeListDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? ProfileImage { get; set; }
        public bool IsActive { get; set; }

        public string? DesignationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? CompanyName { get; set; }
    }
}

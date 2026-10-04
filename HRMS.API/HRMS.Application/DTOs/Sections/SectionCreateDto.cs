using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Sections
{
    public class SectionCreateDto
    {
        public string SectionName { get; set; } = default!;
        public int DepartmentId { get; set; }
    }
}

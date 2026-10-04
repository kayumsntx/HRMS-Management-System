using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Sections
{
    public class SectionResponseDto
    {
        public int Id { get; set; }
        public string SectionName { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}

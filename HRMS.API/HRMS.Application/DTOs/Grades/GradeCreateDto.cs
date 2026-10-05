using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Grades
{
    public class GradeCreateDto
    {
        public string GradeName { get; set; } = default!;
        public string? Description { get; set; }
    }
}

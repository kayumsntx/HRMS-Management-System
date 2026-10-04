using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Designations
{
    public class DesignationUpdateDto
    {
        public string DesignationName { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}

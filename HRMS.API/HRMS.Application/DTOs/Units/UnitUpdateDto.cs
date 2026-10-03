using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Units
{
    public class UnitUpdateDto
    {
        public string UnitName { get; set; } = default!;
        public string? Address { get; set; }
        public int CompanyId { get; set; }
        public bool IsActive { get; set; }
    }
}

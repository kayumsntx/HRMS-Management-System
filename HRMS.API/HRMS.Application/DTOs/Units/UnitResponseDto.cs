using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Units
{
    public class UnitResponseDto
    {
        public int Id { get; set; }
        public string UnitName { get; set; } = default!;
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }
}

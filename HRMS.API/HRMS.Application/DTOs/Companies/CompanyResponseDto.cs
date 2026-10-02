using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Companies
{
    public class CompanyResponseDto
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; } = default!;
        public string CompanyName { get; set; } = default!;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

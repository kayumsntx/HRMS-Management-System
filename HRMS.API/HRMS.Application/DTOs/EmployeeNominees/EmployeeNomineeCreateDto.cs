using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.EmployeeNominees
{
    public class EmployeeNomineeCreateDto
    {
        public int EmployeeId { get; set; }
        public string NomineeName { get; set; } = default!;
        public string Relationship { get; set; } = default!;
        public DateTime? DateOfBirth { get; set; }
        public string? MobileNumber { get; set; }
        public string? NationalIdNo { get; set; }
        public decimal? SharePercentage { get; set; }
        public string? Address { get; set; }
    }
}

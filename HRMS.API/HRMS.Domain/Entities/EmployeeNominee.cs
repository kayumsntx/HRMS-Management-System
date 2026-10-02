using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class EmployeeNominee : BaseEntity
    {
        public string NomineeName { get; set; } = default!;
        public string Relationship { get; set; } = default!;
        public DateTime? DateOfBirth { get; set; }
        public string? MobileNumber { get; set; }
        public string? NationalIdNo { get; set; }
        public decimal? SharePercentage { get; set; }
        public string? Address { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = default!;
    }
}

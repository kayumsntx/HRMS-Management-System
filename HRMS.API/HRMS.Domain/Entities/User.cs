using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class User: BaseEntity
    {
        public string UserName { get; set; } = default!;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = default!;
        public DateTime? LastLoginDate { get; set; }

        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        // Navigation
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

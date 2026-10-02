using HRMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Domain.Entities
{
    public class UserRole:BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = default!;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public int? AssignedBy { get; set; }
    }
}

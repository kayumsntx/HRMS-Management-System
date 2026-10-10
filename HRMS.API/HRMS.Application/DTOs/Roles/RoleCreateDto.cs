using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.DTOs.Roles
{
    public class RoleCreateDto
    {
        public string RoleName { get; set; } = default!;
        public string? Description { get; set; }
    }
}

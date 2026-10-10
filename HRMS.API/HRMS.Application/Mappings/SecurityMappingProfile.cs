using AutoMapper;
using HRMS.Application.DTOs.Roles;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Mappings
{
    public class SecurityMappingProfile : Profile
    {
        public SecurityMappingProfile()
        {
            // Role
            CreateMap<Role, RoleResponseDto>();
            CreateMap<RoleCreateDto, Role>();
            CreateMap<RoleUpdateDto, Role>();

            
        }
    }
}

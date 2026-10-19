using AutoMapper;
using HRMS.Application.DTOs.Roles;
using HRMS.Application.DTOs.UserRoles;
using HRMS.Application.DTOs.Users;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class SecurityMappingProfile : Profile
{
    public SecurityMappingProfile()
    {
        // Role
        CreateMap<Role, RoleResponseDto>();
        CreateMap<RoleCreateDto, Role>();
        CreateMap<RoleUpdateDto, Role>();

        // User
        CreateMap<User, UserResponseDto>();
        CreateMap<UserCreateDto, User>();
        CreateMap<UserUpdateDto, User>();

        // UserRole
        CreateMap<UserRole, UserRoleResponseDto>();
    }
}

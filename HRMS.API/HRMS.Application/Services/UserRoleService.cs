using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.UserRoles;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class UserRoleService : IUserRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public UserRoleService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<UserRoleResponseDto>> GetByUserIdAsync(int userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User), userId);

        var userRoles = (await _unitOfWork.Repository<UserRole>().GetAllAsync())
            .Where(ur => ur.UserId == userId).ToList();
        var roles = await _unitOfWork.Repository<Role>().GetAllAsync();

        var result = _mapper.Map<List<UserRoleResponseDto>>(userRoles);
        foreach (var dto in result)
        {
            dto.UserName = user.UserName;
            dto.RoleName = roles.FirstOrDefault(r => r.Id == dto.RoleId)?.RoleName;
        }

        return result;
    }

    public async Task<UserRoleResponseDto> AssignAsync(UserRoleAssignDto dto)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(dto.UserId)
            ?? throw new NotFoundException(nameof(User), dto.UserId);

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(dto.RoleId)
            ?? throw new NotFoundException(nameof(Role), dto.RoleId);

        if (!role.IsActive)
            throw new BadRequestException("This Role is inactive.");

        var existing = await _unitOfWork.Repository<UserRole>().GetAllAsync();
        if (existing.Any(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId))
            throw new BadRequestException($"User '{user.UserName}' already has the role '{role.RoleName}'.");

        var userRole = new UserRole
        {
            UserId = dto.UserId,
            RoleId = dto.RoleId,
            AssignedDate = DateTime.UtcNow,
            AssignedBy = _currentUser.UserId
        };

        await _unitOfWork.Repository<UserRole>().AddAsync(userRole);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<UserRoleResponseDto>(userRole);
        result.UserName = user.UserName;
        result.RoleName = role.RoleName;
        return result;
    }

    public async Task RemoveAsync(int id)
    {
        var userRole = await _unitOfWork.Repository<UserRole>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(UserRole), id);

        _unitOfWork.Repository<UserRole>().Delete(userRole);
        await _unitOfWork.SaveChangesAsync();
    }
}

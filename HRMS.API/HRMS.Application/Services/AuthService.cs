using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Users;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto)
    {
        var users = await _unitOfWork.Repository<User>().GetAllAsync();
        var user = users.FirstOrDefault(u => string.Equals(u.UserName, dto.UserName, StringComparison.OrdinalIgnoreCase));

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid username or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("This account is inactive.");

        var userRoles = await _unitOfWork.Repository<UserRole>().GetAllAsync();
        var roleIds = userRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();

        var allRoles = await _unitOfWork.Repository<Role>().GetAllAsync();
        var roleNames = allRoles.Where(r => r.IsActive && roleIds.Contains(r.Id)).Select(r => r.RoleName).ToList();

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user, roleNames);

        user.LastLoginDate = DateTime.UtcNow;
        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserLoginResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = token,
            ExpiresAt = expiresAt,
            Roles = roleNames
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User), userId);

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChangesAsync();
    }
}

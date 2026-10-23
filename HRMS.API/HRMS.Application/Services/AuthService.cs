using System.Security.Cryptography;
using System.Text;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Users;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class AuthService : IAuthService
{
    private const int MaxFailedAttempts = 5;     // consecutive failures ...
    private const int LockoutMinutes = 15;       // ... within this window lock the user name (about this long)
    private const int RefreshTokenDays = 7;

    private static string? _dummyHash;           // used to make unknown-user logins take as long as real ones

    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;

    public AuthService(IUnitOfWork uow, IPasswordHasher passwordHasher, IJwtTokenGenerator jwt)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    // =====================================================================
    // LOGIN
    // =====================================================================
    public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto, string? ipAddress, string? userAgent)
    {
        var userName = dto.UserName.Trim();
        var loggedName = Truncate(userName, 100)!;
        var now = DateTime.UtcNow;

        // Lockout: too many consecutive failures recently. Blocked attempts are NOT recorded
        // (otherwise an attacker could extend the lock forever).
        var since = now.AddMinutes(-LockoutMinutes);
        var recent = (await _uow.Repository<LoginHistory>().FindAsync(h => h.UserName == loggedName && h.LoginTime >= since))
            .OrderByDescending(h => h.LoginTime).ThenByDescending(h => h.Id)
            .ToList();

        if (recent.TakeWhile(h => !h.IsSuccess).Count() >= MaxFailedAttempts)
            throw new UnauthorizedException($"Too many failed attempts. Please try again after {LockoutMinutes} minutes.");

        var user = (await _uow.Repository<User>().FindAsync(u => u.UserName == userName)).FirstOrDefault();

        var passwordOk = user != null
            ? _passwordHasher.Verify(dto.Password, user.PasswordHash)
            : _passwordHasher.Verify(dto.Password, _dummyHash ??= _passwordHasher.Hash(Guid.NewGuid().ToString()));

        if (user is null || !passwordOk)
        {
            await RecordAsync(user?.Id, loggedName, false, user is null ? "Unknown user" : "Wrong password", ipAddress, userAgent);
            throw new UnauthorizedException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            await RecordAsync(user.Id, loggedName, false, "Inactive account", ipAddress, userAgent);
            throw new UnauthorizedException("This account is inactive.");
        }

        var response = await IssueTokensAsync(user, ipAddress);

        user.LastLoginDate = now;
        _uow.Repository<User>().Update(user);

        await _uow.Repository<LoginHistory>().AddAsync(new LoginHistory
        {
            UserId = user.Id,
            UserName = loggedName,
            IsSuccess = true,
            IpAddress = Truncate(ipAddress, 50),
            UserAgent = Truncate(userAgent, 500),
            LoginTime = now
        });

        await _uow.SaveChangesAsync();
        return response;
    }

    // =====================================================================
    // REFRESH (rotation: every refresh token works exactly once)
    // =====================================================================
    public async Task<UserLoginResponseDto> RefreshAsync(string refreshToken, string? ipAddress)
    {
        var now = DateTime.UtcNow;
        var hash = Hash(refreshToken);

        var token = (await _uow.Repository<RefreshToken>().FindAsync(t => t.TokenHash == hash)).FirstOrDefault()
            ?? throw new UnauthorizedException("Invalid refresh token.");

        // A revoked token being used again means it was probably stolen: close every session of that user.
        if (token.RevokedAt != null)
        {
            await RevokeAllAsync(token.UserId, now);
            await _uow.SaveChangesAsync();
            throw new UnauthorizedException("Invalid refresh token.");
        }

        if (token.ExpiresAt <= now)
            throw new UnauthorizedException("The refresh token has expired. Please log in again.");

        var user = await _uow.Repository<User>().GetByIdAsync(token.UserId);
        if (user is null || !user.IsActive)
            throw new UnauthorizedException("This account is inactive.");

        var response = await IssueTokensAsync(user, ipAddress);

        token.RevokedAt = now;
        token.ReplacedByHash = Hash(response.RefreshToken);
        token.UpdatedDate = now;
        _uow.Repository<RefreshToken>().Update(token);

        await _uow.SaveChangesAsync();
        return response;
    }

    // =====================================================================
    // LOGOUT
    // =====================================================================
    public async Task LogoutAsync(int userId, string? refreshToken)
    {
        var now = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var hash = Hash(refreshToken);
            var token = (await _uow.Repository<RefreshToken>().FindAsync(t => t.TokenHash == hash && t.UserId == userId)).FirstOrDefault();
            if (token != null && token.RevokedAt == null)
            {
                token.RevokedAt = now;
                token.UpdatedDate = now;
                _uow.Repository<RefreshToken>().Update(token);
            }
        }

        var lastLogin = (await _uow.Repository<LoginHistory>().FindAsync(h => h.UserId == userId && h.IsSuccess && h.LogoutTime == null))
            .OrderByDescending(h => h.LoginTime).FirstOrDefault();

        if (lastLogin != null)
        {
            lastLogin.LogoutTime = now;
            _uow.Repository<LoginHistory>().Update(lastLogin);
        }

        await _uow.SaveChangesAsync();
    }

    // =====================================================================
    // CHANGE PASSWORD (also signs the user out of every other device)
    // =====================================================================
    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _uow.Repository<User>().GetByIdAsync(userId)
            ?? throw new NotFoundException(nameof(User), userId);

        if (!_passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<User>().Update(user);

        await RevokeAllAsync(userId, DateTime.UtcNow);
        await _uow.SaveChangesAsync();
    }

    // ---------------- Private Helpers ----------------

    private async Task<UserLoginResponseDto> IssueTokensAsync(User user, string? ipAddress)
    {
        var now = DateTime.UtcNow;

        var roleIds = (await _uow.Repository<UserRole>().FindAsync(ur => ur.UserId == user.Id)).Select(ur => ur.RoleId).ToList();
        var roleNames = roleIds.Count == 0
            ? new List<string>()
            : (await _uow.Repository<Role>().FindAsync(r => r.IsActive && roleIds.Contains(r.Id))).Select(r => r.RoleName).ToList();

        var (accessToken, accessExpires) = _jwt.GenerateToken(user, roleNames);

        var plain = NewToken();
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(plain),
            ExpiresAt = now.AddDays(RefreshTokenDays),
            CreatedByIp = Truncate(ipAddress, 50)
        };
        await _uow.Repository<RefreshToken>().AddAsync(refresh);

        // housekeeping: forget tokens that expired more than 30 days ago
        var stale = await _uow.Repository<RefreshToken>().FindAsync(t => t.UserId == user.Id && t.ExpiresAt < now.AddDays(-30));
        if (stale.Count > 0)
            _uow.Repository<RefreshToken>().DeleteRange(stale);

        return new UserLoginResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Token = accessToken,
            ExpiresAt = accessExpires,
            RefreshToken = plain,
            RefreshTokenExpiresAt = refresh.ExpiresAt,
            Roles = roleNames
        };
    }

    private async Task RevokeAllAsync(int userId, DateTime now)
    {
        var active = await _uow.Repository<RefreshToken>().FindAsync(t => t.UserId == userId && t.RevokedAt == null);
        foreach (var token in active)
        {
            token.RevokedAt = now;
            token.UpdatedDate = now;
            _uow.Repository<RefreshToken>().Update(token);
        }
    }

    private async Task RecordAsync(int? userId, string userName, bool success, string? reason, string? ip, string? userAgent)
    {
        await _uow.Repository<LoginHistory>().AddAsync(new LoginHistory
        {
            UserId = userId,
            UserName = userName,
            IsSuccess = success,
            FailReason = reason,
            IpAddress = Truncate(ip, 50),
            UserAgent = Truncate(userAgent, 500)
        });
        await _uow.SaveChangesAsync();
    }

    private static string NewToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string? Truncate(string? value, int max)
        => value == null ? null : (value.Length <= max ? value : value.Substring(0, max));
}

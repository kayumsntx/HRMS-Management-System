using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Security;
using HRMS.Application.DTOs.Users;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HRMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, IUserService userService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _userService = userService;
        _currentUser = currentUser;
    }

    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Agent => Request.Headers.UserAgent.ToString();

    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
    {
        var result = await _authService.LoginAsync(dto, Ip, Agent);
        return Ok(ApiResponse<UserLoginResponseDto>.SuccessResponse(result, "Login successful."));
    }

    // Exchanges a refresh token for a new access token + a NEW refresh token (the old one stops working).
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshAsync(dto.RefreshToken, Ip);
        return Ok(ApiResponse<UserLoginResponseDto>.SuccessResponse(result, "Token refreshed."));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Invalid token.");
        await _authService.LogoutAsync(userId, dto.RefreshToken);
        return Ok(ApiResponse<string>.SuccessResponse("", "Logged out successfully."));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Invalid token.");
        var user = await _userService.GetByIdAsync(userId);
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Invalid token.");
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(ApiResponse<string>.SuccessResponse("", "Password changed. Please log in again on your other devices."));
    }
}

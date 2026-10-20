using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Notifications;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(INotificationService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    private int CurrentUserId => _currentUser.UserId ?? throw new UnauthorizedException("Invalid token.");

    // GET: api/notifications?unreadOnly=true   (only MY notifications)
    [HttpGet]
    public async Task<IActionResult> GetMy([FromQuery] bool unreadOnly = false)
    {
        var result = await _service.GetMyAsync(CurrentUserId, unreadOnly);
        return Ok(ApiResponse<List<NotificationResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount()
    {
        var count = await _service.GetUnreadCountAsync(CurrentUserId);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _service.MarkAsReadAsync(CurrentUserId, id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Notification marked as read."));
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var count = await _service.MarkAllAsReadAsync(CurrentUserId);
        return Ok(ApiResponse<int>.SuccessResponse(count, "All notifications marked as read."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(CurrentUserId, id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Notification deleted."));
    }

    // Admin/HR can send a manual notification to one user, or to everyone (UserId = null).
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] NotificationSendDto dto)
    {
        var recipients = await _service.SendAsync(dto);
        return Ok(ApiResponse<int>.SuccessResponse(recipients, $"Notification sent to {recipients} user(s)."));
    }
}

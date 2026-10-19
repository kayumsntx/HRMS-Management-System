using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Notifications;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class NotificationService : INotificationService
{
    private const int MaxResults = 100;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationPublisher _publisher;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, INotificationPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _publisher = publisher;
    }

    public async Task<List<NotificationResponseDto>> GetMyAsync(int userId, bool unreadOnly)
    {
        var items = await _unitOfWork.Repository<Notification>()
            .FindAsync(n => n.UserId == userId && (!unreadOnly || !n.IsRead));

        var latest = items.OrderByDescending(n => n.CreatedDate).ThenByDescending(n => n.Id).Take(MaxResults).ToList();
        return _mapper.Map<List<NotificationResponseDto>>(latest);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        var unread = await _unitOfWork.Repository<Notification>().FindAsync(n => n.UserId == userId && !n.IsRead);
        return unread.Count;
    }

    public async Task MarkAsReadAsync(int userId, int id)
    {
        var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(id);

        // Someone else's notification is reported as "not found" on purpose.
        if (notification is null || notification.UserId != userId)
            throw new NotFoundException(nameof(Notification), id);

        if (notification.IsRead)
            return;

        notification.IsRead = true;
        notification.ReadDate = DateTime.UtcNow;
        notification.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<Notification>().Update(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var unread = await _unitOfWork.Repository<Notification>().FindAsync(n => n.UserId == userId && !n.IsRead);
        if (unread.Count == 0)
            return 0;

        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadDate = now;
            notification.UpdatedDate = now;
            _unitOfWork.Repository<Notification>().Update(notification);
        }

        await _unitOfWork.SaveChangesAsync();
        return unread.Count;
    }

    public async Task DeleteAsync(int userId, int id)
    {
        var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(id);

        if (notification is null || notification.UserId != userId)
            throw new NotFoundException(nameof(Notification), id);

        _unitOfWork.Repository<Notification>().Delete(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> SendAsync(NotificationSendDto dto)
    {
        List<int> userIds;

        if (dto.UserId.HasValue)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(dto.UserId.Value)
                ?? throw new NotFoundException(nameof(User), dto.UserId.Value);

            if (!user.IsActive)
                throw new BadRequestException("This User is inactive.");

            userIds = new List<int> { user.Id };
        }
        else
        {
            userIds = (await _unitOfWork.Repository<User>().FindAsync(u => u.IsActive)).Select(u => u.Id).ToList();
        }

        if (userIds.Count == 0)
            throw new BadRequestException("There is no active user to notify.");

        var category = NotificationCategory.General;
        if (!string.IsNullOrWhiteSpace(dto.NotificationType) &&
            !Enum.TryParse(dto.NotificationType, true, out category))
            throw new BadRequestException("Invalid Notification Type.");

        await _publisher.NotifyUsersAsync(userIds, category, dto.Title.Trim(), dto.Message.Trim(), dto.LinkUrl);
        await _unitOfWork.SaveChangesAsync();

        return userIds.Count;
    }
}

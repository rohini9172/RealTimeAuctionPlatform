using Auction.Application.DTOs.Notifications;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Exceptions;

namespace Auction.Infrastructure.Services;

// Business logic for notifications: get my notifications, mark as read.
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(int userId, CancellationToken ct = default)
    {
        return await _notificationRepository.GetByUserAsync(userId, ct);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default)
    {
        // Ownership enforced in repository
        await _notificationRepository.MarkAsReadAsync(notificationId, userId, ct);
    }
}
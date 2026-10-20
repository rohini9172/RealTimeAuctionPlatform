using Auction.Application.DTOs.Notifications;

namespace Auction.Application.Interfaces;

// Business operations for notifications.
public interface INotificationService
{
    Task<List<NotificationDto>> GetMyNotificationsAsync(int userId, CancellationToken ct = default);
    Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default);
}

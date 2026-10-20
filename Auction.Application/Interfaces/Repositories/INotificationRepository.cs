using Auction.Application.DTOs.Notifications;

namespace Auction.Application.Interfaces.Repositories;

// Defines all notification-related data operations.
public interface INotificationRepository
{
    Task<List<NotificationDto>> GetByUserAsync(int userId, CancellationToken ct = default);

    Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default);

    // Create a notification — called by services and background jobs
    Task CreateAsync(int userId, int? auctionId, string message, int type, CancellationToken ct = default);
}

using Auction.Application.DTOs.Notifications;
using Auction.Application.Interfaces.Repositories;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories;

// Data-access implementation for notifications.
public class NotificationRepository : INotificationRepository
{
    private readonly AuctionDbContext _dbContext;

    public NotificationRepository(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<NotificationDto>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                AuctionId = n.AuctionId,
                Message = n.Message,
                Type = ((Auction.Domain.Enums.NotificationType)n.Type).ToString(),
                IsRead = n.IsRead,
                CreatedAtUtc = n.CreatedAtUtc
            })
            .ToListAsync(ct);
    }

    public async Task MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification == null)
        {
            throw new Auction.Domain.Exceptions.NotFoundException("Notification not found.");
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task CreateAsync(int userId, int? auctionId, string message, int type, CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            AuctionId = auctionId,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(ct);
    }
}
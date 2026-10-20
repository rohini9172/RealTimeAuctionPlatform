namespace Auction.Application.DTOs.Notifications;

// Returned in the notifications list.
public class NotificationDto
{
    public int Id { get; set; }
    public int? AuctionId { get; set; }
    public string Message { get; set; } = string.Empty;

    // String representation of NotificationType enum
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

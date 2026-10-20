namespace Auction.Application.DTOs.WatchList;

// Returned in the watchlist — includes auction summary so the
// frontend can display it without a second API call.
public class WatchListItemDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public string AuctionTitle { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string AuctionStatus { get; set; } = string.Empty;
    public DateTime EndTime { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

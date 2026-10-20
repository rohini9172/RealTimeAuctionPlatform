namespace Auction.Application.DTOs.Auctions;

// Returned for auction list pages — lightweight, no bid history.
public class AuctionSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;

    // First image URL for thumbnail display, null if no images
    public string? ThumbnailUrl { get; set; }
}

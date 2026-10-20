namespace Auction.Application.DTOs.Auctions;

// Sent by a Seller when updating an existing auction.
// Only allowed while the auction is still Scheduled (no bids yet).
public class UpdateAuctionRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal MinIncrement { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}

namespace Auction.Application.DTOs.Auctions;

// Sent by a Seller when creating a new auction.
public class CreateAuctionRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal MinIncrement { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Optional list of image URLs for the auction
    public List<string> ImageUrls { get; set; } = new();
}

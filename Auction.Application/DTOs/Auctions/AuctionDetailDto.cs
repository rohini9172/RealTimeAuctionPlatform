namespace Auction.Application.DTOs.Auctions;

// Returned for the auction detail page — full information including images.
public class AuctionDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal MinIncrement { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? HighestBidderName { get; set; }
    public string? WinnerName { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}

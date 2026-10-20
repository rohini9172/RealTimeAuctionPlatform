namespace Auction.Application.DTOs.Bids;

// Returned in bid history lists.
public class BidDto
{
    public int Id { get; set; }
    public int AuctionId { get; set; }
    public string BidderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime BidTimeUtc { get; set; }
    public bool IsWinningBid { get; set; }
}

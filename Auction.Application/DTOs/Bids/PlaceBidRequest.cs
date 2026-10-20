namespace Auction.Application.DTOs.Bids;

// Sent by a Bidder when placing a bid on an auction.
public class PlaceBidRequest
{
    public decimal Amount { get; set; }
}

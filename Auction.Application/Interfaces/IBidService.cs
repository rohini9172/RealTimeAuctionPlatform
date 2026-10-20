using Auction.Application.DTOs.Bids;

namespace Auction.Application.Interfaces;

// Business operations for bidding.
public interface IBidService
{
    // Place a bid — handles all validation, concurrency, and notifications
    Task<BidDto> PlaceBidAsync(int auctionId, int bidderId, PlaceBidRequest request, CancellationToken ct = default);

    Task<List<BidDto>> GetAuctionBidsAsync(int auctionId, CancellationToken ct = default);
    Task<List<BidDto>> GetMyBidsAsync(int bidderId, CancellationToken ct = default);
}

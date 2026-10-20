using Auction.Application.DTOs.Bids;

namespace Auction.Application.Interfaces.Repositories;

// Defines all bid-related data operations.
public interface IBidRepository
{
    // Get all bids for an auction, ordered by amount descending
    Task<List<BidDto>> GetByAuctionAsync(int auctionId, CancellationToken ct = default);

    // Get all bids placed by a specific user
    Task<List<BidDto>> GetByBidderAsync(int bidderId, CancellationToken ct = default);

    // Place a new bid — this is called inside a transaction in the service layer.
    // Returns the new bid's ID.
    Task<int> CreateAsync(int auctionId, int bidderId, decimal amount, CancellationToken ct = default);

    // Mark a specific bid as the winning bid (called when auction ends)
    Task MarkWinningBidAsync(int bidId, CancellationToken ct = default);
}

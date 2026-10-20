using Auction.Application.DTOs.Auctions;

namespace Auction.Application.Interfaces.Repositories;

// Defines all auction-related data operations.
public interface IAuctionRepository
{
    Task<AuctionDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<PagedResult<AuctionSummaryDto>> GetPagedAsync(AuctionQueryParams query, CancellationToken ct = default);

    // Get all auctions created by a specific seller
    Task<List<AuctionSummaryDto>> GetBySellerAsync(int sellerId, CancellationToken ct = default);

    // Create a new auction — returns the new auction's ID
    Task<int> CreateAsync(int sellerId, CreateAuctionRequest request, CancellationToken ct = default);

    // Update an existing auction's fields
    Task UpdateAsync(int id, UpdateAuctionRequest request, CancellationToken ct = default);

    // Cancel an auction — sets status to Cancelled
    Task CancelAsync(int id, CancellationToken ct = default);

    // Check who owns the auction (used for ownership validation)
    Task<int?> GetSellerIdAsync(int auctionId, CancellationToken ct = default);

    // Get the current status integer of an auction
    Task<int?> GetStatusAsync(int auctionId, CancellationToken ct = default);
}

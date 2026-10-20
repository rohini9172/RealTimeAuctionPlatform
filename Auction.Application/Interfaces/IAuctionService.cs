using Auction.Application.DTOs.Auctions;

namespace Auction.Application.Interfaces;

// Business operations for auctions.
public interface IAuctionService
{
    Task<AuctionDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<AuctionSummaryDto>> GetPagedAsync(AuctionQueryParams query, CancellationToken ct = default);
    Task<List<AuctionSummaryDto>> GetMyAuctionsAsync(int sellerId, CancellationToken ct = default);
    Task<AuctionDetailDto> CreateAsync(int sellerId, CreateAuctionRequest request, CancellationToken ct = default);
    Task<AuctionDetailDto> UpdateAsync(int auctionId, int sellerId, UpdateAuctionRequest request, CancellationToken ct = default);
    Task CancelAsync(int auctionId, int sellerId, CancellationToken ct = default);
}

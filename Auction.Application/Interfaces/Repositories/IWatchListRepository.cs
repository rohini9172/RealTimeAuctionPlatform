using Auction.Application.DTOs.WatchList;

namespace Auction.Application.Interfaces.Repositories;

// Defines all watchlist-related data operations.
public interface IWatchListRepository
{
    Task<List<WatchListItemDto>> GetByUserAsync(int userId, CancellationToken ct = default);

    // Returns true if the entry was added, false if it already existed
    Task<bool> AddAsync(int userId, int auctionId, CancellationToken ct = default);

    Task RemoveAsync(int userId, int auctionId, CancellationToken ct = default);

    Task<bool> ExistsAsync(int userId, int auctionId, CancellationToken ct = default);
}

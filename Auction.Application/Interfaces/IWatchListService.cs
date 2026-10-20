using Auction.Application.DTOs.WatchList;

namespace Auction.Application.Interfaces;

// Business operations for the watchlist.
public interface IWatchListService
{
    Task<List<WatchListItemDto>> GetMyWatchListAsync(int userId, CancellationToken ct = default);
    Task AddAsync(int userId, int auctionId, CancellationToken ct = default);
    Task RemoveAsync(int userId, int auctionId, CancellationToken ct = default);
}

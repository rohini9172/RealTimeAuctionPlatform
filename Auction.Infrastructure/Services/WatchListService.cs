using Auction.Application.DTOs.WatchList;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Exceptions;

namespace Auction.Infrastructure.Services;

// Business logic for watchlists: add, remove, get my watchlist.
// Ownership and duplicate prevention enforced here.
public class WatchListService : IWatchListService
{
    private readonly IWatchListRepository _watchListRepository;

    public WatchListService(IWatchListRepository watchListRepository)
    {
        _watchListRepository = watchListRepository;
    }

    public async Task<List<WatchListItemDto>> GetMyWatchListAsync(int userId, CancellationToken ct = default)
    {
        return await _watchListRepository.GetByUserAsync(userId, ct);
    }

    public async Task AddAsync(int userId, int auctionId, CancellationToken ct = default)
    {
        var added = await _watchListRepository.AddAsync(userId, auctionId, ct);
        if (!added)
        {
            throw new DomainException("This auction is already in your watchlist.");
        }
    }

    public async Task RemoveAsync(int userId, int auctionId, CancellationToken ct = default)
    {
        var exists = await _watchListRepository.ExistsAsync(userId, auctionId, ct);
        if (!exists)
        {
            throw new NotFoundException("Watchlist entry not found.");
        }

        await _watchListRepository.RemoveAsync(userId, auctionId, ct);
    }
}
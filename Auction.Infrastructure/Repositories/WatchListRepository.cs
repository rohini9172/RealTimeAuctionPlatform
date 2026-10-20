using Auction.Application.DTOs.WatchList;
using Auction.Application.Interfaces.Repositories;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories;

// Data-access implementation for watchlists.
// The unique constraint on (UserId, AuctionId) is enforced both here and in the database.
public class WatchListRepository : IWatchListRepository
{
    private readonly AuctionDbContext _dbContext;

    public WatchListRepository(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WatchListItemDto>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        return await _dbContext.WatchLists
            .Where(w => w.UserId == userId)
            .Include(w => w.Auction)
            .ThenInclude(a => a.AuctionImages)
            .OrderByDescending(w => w.CreatedAtUtc)
            .Select(w => new WatchListItemDto
            {
                Id = w.Id,
                AuctionId = w.AuctionId,
                AuctionTitle = w.Auction.Title,
                CurrentPrice = w.Auction.CurrentPrice,
                AuctionStatus = ((Auction.Domain.Enums.AuctionStatus)w.Auction.Status).ToString(),
                EndTime = w.Auction.EndTime,
                ThumbnailUrl = w.Auction.AuctionImages.FirstOrDefault() != null
                    ? w.Auction.AuctionImages.FirstOrDefault()!.ImageUrl
                    : null,
                CreatedAtUtc = w.CreatedAtUtc
            })
            .ToListAsync(ct);
    }

    public async Task<bool> AddAsync(int userId, int auctionId, CancellationToken ct = default)
    {
        var exists = await _dbContext.WatchLists
            .AnyAsync(w => w.UserId == userId && w.AuctionId == auctionId, ct);

        if (exists)
        {
            return false; // duplicate — already watched
        }

        var watchList = new WatchList
        {
            UserId = userId,
            AuctionId = auctionId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.WatchLists.Add(watchList);
        await _dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task RemoveAsync(int userId, int auctionId, CancellationToken ct = default)
    {
        var entry = await _dbContext.WatchLists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.AuctionId == auctionId, ct);

        if (entry != null)
        {
            _dbContext.WatchLists.Remove(entry);
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(int userId, int auctionId, CancellationToken ct = default)
    {
        return await _dbContext.WatchLists
            .AnyAsync(w => w.UserId == userId && w.AuctionId == auctionId, ct);
    }
}
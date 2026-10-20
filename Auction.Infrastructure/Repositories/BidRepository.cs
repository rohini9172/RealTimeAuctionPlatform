using Auction.Application.DTOs.Bids;
using Auction.Application.Interfaces.Repositories;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories;

// Data-access implementation for bids.
// All bid writes happen inside a transaction managed by the service layer.
public class BidRepository : IBidRepository
{
    private readonly AuctionDbContext _dbContext;

    public BidRepository(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BidDto>> GetByAuctionAsync(int auctionId, CancellationToken ct = default)
    {
        return await _dbContext.Bids
            .Where(b => b.AuctionId == auctionId)
            .Include(b => b.Bidder)
            .OrderByDescending(b => b.Amount)
            .ThenByDescending(b => b.BidTimeUtc)
            .Select(b => new BidDto
            {
                Id = b.Id,
                AuctionId = b.AuctionId,
                BidderName = b.Bidder.FullName,
                Amount = b.Amount,
                BidTimeUtc = b.BidTimeUtc,
                IsWinningBid = b.IsWinningBid
            })
            .ToListAsync(ct);
    }

    public async Task<List<BidDto>> GetByBidderAsync(int bidderId, CancellationToken ct = default)
    {
        return await _dbContext.Bids
            .Where(b => b.BidderId == bidderId)
            .Include(b => b.Auction)
            .OrderByDescending(b => b.BidTimeUtc)
            .Select(b => new BidDto
            {
                Id = b.Id,
                AuctionId = b.AuctionId,
                BidderName = b.Auction.Title, // reuse BidderName field for auction title context
                Amount = b.Amount,
                BidTimeUtc = b.BidTimeUtc,
                IsWinningBid = b.IsWinningBid
            })
            .ToListAsync(ct);
    }

    public async Task<int> CreateAsync(int auctionId, int bidderId, decimal amount, CancellationToken ct = default)
    {
        var bid = new Bid
        {
            AuctionId = auctionId,
            BidderId = bidderId,
            Amount = amount,
            BidTimeUtc = DateTime.UtcNow,
            IsWinningBid = true // temporary; service updates old winning bids
        };

        _dbContext.Bids.Add(bid);
        await _dbContext.SaveChangesAsync(ct);
        return bid.Id;
    }

    public async Task MarkWinningBidAsync(int bidId, CancellationToken ct = default)
    {
        // Clear all winning bids, then mark the specified one
        var bids = _dbContext.Bids.Where(b => b.IsWinningBid);
        foreach (var b in bids)
        {
            b.IsWinningBid = false;
        }

        var target = await _dbContext.Bids.FindAsync(new object?[] { bidId }, ct);
        if (target != null)
        {
            target.IsWinningBid = true;
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
using Auction.Application.DTOs.Bids;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Enums;
using Auction.Domain.Exceptions;
using Auction.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Services;

// Business logic for bidding: place bid, get bid history, my bids.
// Concurrency handling and atomic price updates live here.
public class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly AuctionDbContext _dbContext;

    public BidService(IBidRepository bidRepository, IAuctionRepository auctionRepository, AuctionDbContext dbContext)
    {
        _bidRepository = bidRepository;
        _auctionRepository = auctionRepository;
        _dbContext = dbContext;
    }

    public async Task<BidDto> PlaceBidAsync(int auctionId, int bidderId, PlaceBidRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
        {
            throw new DomainException("Bid amount must be greater than zero.");
        }

        // Use a transaction so the bid insert and auction price update are atomic.
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            // Reload the auction WITH RowVersion inside the transaction so we
            // can detect concurrent updates. RowVersion is the concurrency token.
            var auction = await _dbContext.Auctions
                .Where(a => a.Id == auctionId)
                .FirstOrDefaultAsync(ct);

            if (auction == null)
            {
                throw new NotFoundException("Auction not found.");
            }

            // Rule: only live auctions accept bids
            if (auction.Status != (int)AuctionStatus.Live)
            {
                throw new DomainException("Bids are only accepted while the auction is live.");
            }

            // Rule: end time must not have passed
            if (auction.EndTime <= DateTime.UtcNow)
            {
                throw new DomainException("This auction has already ended.");
            }

            // Rule: seller cannot bid on their own auction
            if (auction.SellerId == bidderId)
            {
                throw new DomainException("Sellers cannot bid on their own auctions.");
            }

            // Determine minimum acceptable bid
            decimal minimumBid;
            if (auction.HighestBidderId == null)
            {
                // First bid must meet starting price
                minimumBid = auction.StartingPrice;
            }
            else
            {
                // Subsequent bids must be current price + min increment
                minimumBid = auction.CurrentPrice + auction.MinIncrement;
            }

            if (request.Amount < minimumBid)
            {
                throw new DomainException(
                    $"Bid must be at least ₹{minimumBid:N2} (current price + minimum increment).");
            }

            // Create the bid
            var bid = new Auction.Infrastructure.Persistence.Entities.Bid
            {
                AuctionId = auctionId,
                BidderId = bidderId,
                Amount = request.Amount,
                BidTimeUtc = DateTime.UtcNow,
                IsWinningBid = true // temporary; previous winning bids are unset below
            };

            _dbContext.Bids.Add(bid);

            // Unset any previous winning bid for this auction
            var previousWinningBids = _dbContext.Bids
                .Where(b => b.AuctionId == auctionId && b.IsWinningBid && b.Id != bid.Id);
            foreach (var pb in previousWinningBids)
            {
                pb.IsWinningBid = false;
            }

            // Update auction current price and highest bidder atomically
            auction.CurrentPrice = request.Amount;
            auction.HighestBidderId = bidderId;

            var bidderName = await _dbContext.Users
                .Where(u => u.Id == bidderId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(ct) ?? string.Empty;

            // SaveChanges will include RowVersion in the UPDATE WHERE clause.
            // If another request updated the auction first, EF throws DbUpdateConcurrencyException.
            await _dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            return new BidDto
            {
                Id = bid.Id,
                AuctionId = bid.AuctionId,
                BidderName = bidderName,
                Amount = bid.Amount,
                BidTimeUtc = bid.BidTimeUtc,
                IsWinningBid = true
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(ct);
            throw new ConflictException("Concurrency conflict: another bid was placed at the same time. Please refresh and try again.");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<List<BidDto>> GetAuctionBidsAsync(int auctionId, CancellationToken ct = default)
    {
        return await _bidRepository.GetByAuctionAsync(auctionId, ct);
    }

    public async Task<List<BidDto>> GetMyBidsAsync(int bidderId, CancellationToken ct = default)
    {
        return await _bidRepository.GetByBidderAsync(bidderId, ct);
    }
}
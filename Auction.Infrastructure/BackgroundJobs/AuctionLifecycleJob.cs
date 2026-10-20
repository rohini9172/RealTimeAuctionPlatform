using Auction.Domain.Enums;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auction.Infrastructure.BackgroundJobs;

// Background job that manages the auction lifecycle:
//   1. Activates scheduled auctions when their start time arrives.
//   2. Ends live auctions when their end time arrives and determines the winner.
//   3. Marks auctions without bids as ended without a winner.
//   4. Creates relevant notifications for auction started, ended, won, and outbid events.
//
// The job is designed to be idempotent: running it multiple times for the same
// auction produces the same result, so repeated Hangfire executions are safe.
public class AuctionLifecycleJob
{
    private readonly AuctionDbContext _dbContext;
    private readonly ILogger<AuctionLifecycleJob> _logger;

    public AuctionLifecycleJob(
        AuctionDbContext dbContext,
        ILogger<AuctionLifecycleJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Recurring job that runs every minute to check for auctions to activate or end.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessAuctionLifecycleAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Auction lifecycle job started at {Time}", DateTime.UtcNow);

        await ActivateScheduledAuctionsAsync(ct);
        await EndExpiredAuctionsAsync(ct);

        _logger.LogInformation("Auction lifecycle job completed at {Time}", DateTime.UtcNow);
    }

    /// <summary>
    /// Activates auctions whose start time has arrived and are still Scheduled.
    /// </summary>
    private async Task ActivateScheduledAuctionsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var auctionsToActivate = await _dbContext.Auctions
            .Where(a => a.Status == (int)AuctionStatus.Scheduled && a.StartTime <= now)
            .ToListAsync(ct);

        foreach (var auction in auctionsToActivate)
        {
            var current = await _dbContext.Auctions.FindAsync(new object?[] { auction.Id }, ct);
            if (current == null || current.Status != (int)AuctionStatus.Scheduled)
            {
                continue;
            }

            current.Status = (int)AuctionStatus.Live;
            _logger.LogInformation("Activated auction {AuctionId} ({Title})", current.Id, current.Title);

            await CreateAuctionStartedNotificationAsync(current.Id, ct);
        }

        if (auctionsToActivate.Any())
        {
            try
            {
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while activating auctions.");
            }
        }
    }
    /// <summary>
    /// Ends live auctions whose end time has passed, determines the winner,
    /// and creates notifications.
    /// </summary>
    private async Task EndExpiredAuctionsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var auctionsToEnd = await _dbContext.Auctions
            .Where(a => a.Status == (int)AuctionStatus.Live && a.EndTime <= now)
            .ToListAsync(ct);

        foreach (var auction in auctionsToEnd)
        {
            var current = await _dbContext.Auctions.FindAsync(new object?[] { auction.Id }, ct);
            if (current == null || current.Status != (int)AuctionStatus.Live)
            {
                continue;
            }

            var highestBid = await _dbContext.Bids
                .Where(b => b.AuctionId == current.Id)
                .OrderByDescending(b => b.Amount)
                .ThenByDescending(b => b.BidTimeUtc)
                .FirstOrDefaultAsync(ct);

            if (highestBid != null)
            {
                current.WinnerId = highestBid.BidderId;
                current.Status = (int)AuctionStatus.Ended;
                current.HighestBidderId = highestBid.BidderId;

                _logger.LogInformation(
                    "Ended auction {AuctionId} ({Title}). Winner: User {WinnerId}, Winning bid: {Amount}",
                    current.Id, current.Title, current.WinnerId, highestBid.Amount);

                await CreateNotificationAsync(
                    current.WinnerId.Value,
                    current.Id,
                    $"Congratulations! You won the auction '{current.Title}' with a bid of {highestBid.Amount:C}.",
                    (int)NotificationType.AuctionWon,
                    ct);

                await CreateNotificationAsync(
                    current.SellerId,
                    current.Id,
                    $"Your auction '{current.Title}' has ended. The winner is {await GetUserNameAsync(current.WinnerId.Value, ct)}.",
                    (int)NotificationType.AuctionEnded,
                    ct);

                await NotifyOutbidBiddersAsync(current.Id, current.WinnerId.Value, ct);
            }
            else
            {
                current.Status = (int)AuctionStatus.Ended;
                current.WinnerId = null;
                current.HighestBidderId = null;

                _logger.LogInformation(
                    "Ended auction {AuctionId} ({Title}) with no bids.", current.Id, current.Title);

                await CreateNotificationAsync(
                    current.SellerId,
                    current.Id,
                    $"Your auction '{current.Title}' has ended with no bids.",
                    (int)NotificationType.AuctionEnded,
                    ct);
            }
        }

        if (auctionsToEnd.Any())
        {
            try
            {
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while ending auctions.");
            }
        }
    }
    /// <summary>
    /// Notifies all bidders who were outbid (everyone except the winner).
    /// </summary>
    private async Task NotifyOutbidBiddersAsync(int auctionId, int winnerId, CancellationToken ct)
    {
        var outbidBidders = await _dbContext.Bids
            .Where(b => b.AuctionId == auctionId && b.BidderId != winnerId)
            .Select(b => b.BidderId)
            .Distinct()
            .ToListAsync(ct);

        var auction = await _dbContext.Auctions
            .Where(a => a.Id == auctionId)
            .Select(a => new { a.Id, a.Title })
            .FirstOrDefaultAsync(ct);

        if (auction == null) return;

        foreach (var bidderId in outbidBidders)
        {
            await CreateNotificationAsync(
                bidderId,
                auctionId,
                $"You were outbid on the auction '{auction.Title}'. The auction has ended.",
                (int)NotificationType.OutBid,
                ct);
        }
    }

    /// <summary>
    /// Creates an "Auction Started" notification for users watching the auction.
    /// </summary>
    private async Task CreateAuctionStartedNotificationAsync(int auctionId, CancellationToken ct)
    {
        var watchers = await _dbContext.WatchLists
            .Where(w => w.AuctionId == auctionId)
            .Select(w => w.UserId)
            .ToListAsync(ct);

        var auction = await _dbContext.Auctions
            .Where(a => a.Id == auctionId)
            .Select(a => new { a.Id, a.Title })
            .FirstOrDefaultAsync(ct);

        if (auction == null) return;

        foreach (var userId in watchers)
        {
            await CreateNotificationAsync(
                userId,
                auctionId,
                $"The auction '{auction.Title}' is now live and accepting bids.",
                (int)NotificationType.AuctionStarted,
                ct);
        }
    }

    private async Task CreateNotificationAsync(
        int userId,
        int? auctionId,
        string message,
        int type,
        CancellationToken ct)
    {
        var exists = await _dbContext.Notifications
            .AnyAsync(n =>
                n.UserId == userId &&
                n.AuctionId == auctionId &&
                n.Message == message &&
                n.CreatedAtUtc >= DateTime.UtcNow.AddMinutes(-5),
            ct);

        if (exists) return;

        var notification = new Notification
        {
            UserId = userId,
            AuctionId = auctionId,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
    }

    private async Task<string?> GetUserNameAsync(int userId, CancellationToken ct)
    {
        return await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(ct);
    }
}
namespace Auction.Domain.Enums;

// Tracks the lifecycle of an auction from creation to completion.
// Scheduled -> Live -> Ended (or Cancelled before it goes live)
public enum AuctionStatus
{
    Scheduled = 1,  // Created but start time hasn't arrived yet
    Live = 2,       // Currently accepting bids
    Ended = 3,      // Past end time, winner determined
    Cancelled = 4   // Cancelled by seller or admin before ending
}

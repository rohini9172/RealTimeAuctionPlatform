namespace Auction.Domain.Enums;

// Categorizes notifications so the frontend can display the right icon/message.
public enum NotificationType
{
    AuctionStarted = 1,   // Auction went live
    AuctionEnded = 2,     // Auction finished
    OutBid = 3,           // Someone placed a higher bid on your bid
    AuctionWon = 4,       // You won the auction
    AuctionCancelled = 5  // Auction was cancelled
}

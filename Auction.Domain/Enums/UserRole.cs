namespace Auction.Domain.Enums;

// Defines the three roles a user can have in the system.
// Admin manages the platform, Seller creates auctions, Bidder places bids.
public enum UserRole
{
    Admin = 1,
    Seller = 2,
    Bidder = 3
}

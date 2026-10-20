using System;
using System.Collections.Generic;

namespace Auction.Infrastructure.Persistence.Entities;

public partial class Bid
{
    public int Id { get; set; }

    public int AuctionId { get; set; }

    public int BidderId { get; set; }

    public decimal Amount { get; set; }

    public DateTime BidTimeUtc { get; set; }

    public bool IsWinningBid { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual User Bidder { get; set; } = null!;
}

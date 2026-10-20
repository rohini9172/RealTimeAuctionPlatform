using System;
using System.Collections.Generic;

namespace Auction.Infrastructure.Persistence.Entities;

public partial class AutoBid
{
    public int Id { get; set; }

    public int AuctionId { get; set; }

    public int BidderId { get; set; }

    public decimal MaximumAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsActive { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual User Bidder { get; set; } = null!;
}

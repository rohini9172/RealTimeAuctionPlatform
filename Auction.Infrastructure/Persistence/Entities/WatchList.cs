using System;
using System.Collections.Generic;

namespace Auction.Infrastructure.Persistence.Entities;

public partial class WatchList
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AuctionId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual Auction Auction { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}

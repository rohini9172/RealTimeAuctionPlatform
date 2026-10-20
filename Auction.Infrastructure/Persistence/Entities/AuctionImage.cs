using System;
using System.Collections.Generic;

namespace Auction.Infrastructure.Persistence.Entities;

public partial class AuctionImage
{
    public int Id { get; set; }

    public int AuctionId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Auction Auction { get; set; } = null!;
}

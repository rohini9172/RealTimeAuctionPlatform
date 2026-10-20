using System;
using System.Collections.Generic;

namespace Auction.Infrastructure.Persistence.Entities;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? AuctionId { get; set; }

    public string Message { get; set; } = null!;

    public int Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual Auction? Auction { get; set; }

    public virtual User User { get; set; } = null!;
}

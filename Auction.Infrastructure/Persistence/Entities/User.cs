using System;
using System.Collections.Generic;

namespace Auction.Infrastructure.Persistence.Entities;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int Role { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Auction> AuctionHighestBidders { get; set; } = new List<Auction>();

    public virtual ICollection<Auction> AuctionSellers { get; set; } = new List<Auction>();

    public virtual ICollection<Auction> AuctionWinners { get; set; } = new List<Auction>();

    public virtual ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<WatchList> WatchLists { get; set; } = new List<WatchList>();
}

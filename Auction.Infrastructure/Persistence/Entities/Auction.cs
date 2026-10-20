using System;
using System.Collections.Generic;

namespace Auction.Infrastructure.Persistence.Entities;

public partial class Auction
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal StartingPrice { get; set; }

    public decimal MinIncrement { get; set; }

    public decimal CurrentPrice { get; set; }

    public int? HighestBidderId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Status { get; set; }

    public int? WinnerId { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<AuctionImage> AuctionImages { get; set; } = new List<AuctionImage>();

    public virtual ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual Category Category { get; set; } = null!;

    public virtual User? HighestBidder { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual User Seller { get; set; } = null!;

    public virtual ICollection<WatchList> WatchLists { get; set; } = new List<WatchList>();

    public virtual User? Winner { get; set; }
}

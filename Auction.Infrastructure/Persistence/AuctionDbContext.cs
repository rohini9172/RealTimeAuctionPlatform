using Auction.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

// We alias the Entities namespace to resolve the conflict between
// the 'Auction' entity class and the 'Auction' root namespace.
// Without this alias, 'Auction' is ambiguous — the compiler sees
// both the namespace and the class with the same name.
using Entities = Auction.Infrastructure.Persistence.Entities;

namespace Auction.Infrastructure.Persistence;

public partial class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions<AuctionDbContext> options)
        : base(options)
    {
    }

    // DbSets — one per table. EF uses these to build queries.
    public virtual DbSet<Entities.Auction> Auctions { get; set; }
    public virtual DbSet<AuctionImage> AuctionImages { get; set; }
    public virtual DbSet<AutoBid> AutoBids { get; set; }
    public virtual DbSet<Bid> Bids { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<WatchList> WatchLists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Auction entity configuration ---
        modelBuilder.Entity<Entities.Auction>(entity =>
        {
            // Indexes (mirroring what we created in 03_IndexesAndConstraints.sql)
            entity.HasIndex(e => e.CategoryId, "IX_Auctions_CategoryId");
            entity.HasIndex(e => e.EndTime, "IX_Auctions_EndTime");
            entity.HasIndex(e => e.SellerId, "IX_Auctions_SellerId");
            entity.HasIndex(e => e.StartTime, "IX_Auctions_StartTime");
            entity.HasIndex(e => e.Status, "IX_Auctions_Status");
            entity.HasIndex(e => new { e.Status, e.EndTime }, "IX_Auctions_Status_EndTime");

            // Column types
            entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinIncrement).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StartingPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Status).HasDefaultValue(1);

            // RowVersion is the optimistic concurrency token.
            // EF will include it in UPDATE WHERE clauses automatically.
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // Foreign key relationships — NO ACTION to avoid multiple cascade paths
            entity.HasOne(d => d.Category).WithMany(p => p.Auctions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auctions_Category");

            entity.HasOne(d => d.HighestBidder).WithMany(p => p.AuctionHighestBidders)
                .HasForeignKey(d => d.HighestBidderId)
                .HasConstraintName("FK_Auctions_HighestBidder");

            entity.HasOne(d => d.Seller).WithMany(p => p.AuctionSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auctions_Seller");

            entity.HasOne(d => d.Winner).WithMany(p => p.AuctionWinners)
                .HasForeignKey(d => d.WinnerId)
                .HasConstraintName("FK_Auctions_Winner");
        });

        // --- AuctionImage entity configuration ---
        modelBuilder.Entity<AuctionImage>(entity =>
        {
            entity.Property(e => e.ImageUrl).HasMaxLength(1000);

            entity.HasOne(d => d.Auction).WithMany(p => p.AuctionImages)
                .HasForeignKey(d => d.AuctionId)
                .HasConstraintName("FK_AuctionImages_Auction");
        });

        // --- AutoBid entity configuration ---
        modelBuilder.Entity<AutoBid>(entity =>
        {
            entity.HasIndex(e => new { e.AuctionId, e.IsActive }, "IX_AutoBids_AuctionId_IsActive");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaximumAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Auction).WithMany(p => p.AutoBids)
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AutoBids_Auction");

            entity.HasOne(d => d.Bidder).WithMany(p => p.AutoBids)
                .HasForeignKey(d => d.BidderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AutoBids_Bidder");
        });

        // --- Bid entity configuration ---
        modelBuilder.Entity<Bid>(entity =>
        {
            entity.HasIndex(e => e.AuctionId, "IX_Bids_AuctionId");
            entity.HasIndex(e => new { e.AuctionId, e.Amount }, "IX_Bids_AuctionId_Amount").IsDescending(false, true);
            entity.HasIndex(e => e.BidderId, "IX_Bids_BidderId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BidTimeUtc).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Auction).WithMany(p => p.Bids)
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bids_Auction");

            entity.HasOne(d => d.Bidder).WithMany(p => p.Bids)
                .HasForeignKey(d => d.BidderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bids_Bidder");
        });

        // --- Category entity configuration ---
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Categories_Name").IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        // --- Notification entity configuration ---
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");
            entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_Notifications_UserId_IsRead");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Message).HasMaxLength(500);

            entity.HasOne(d => d.Auction).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.AuctionId)
                .HasConstraintName("FK_Notifications_Auction");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_User");
        });

        // --- User entity configuration ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.Role).HasDefaultValue(3);
        });

        // --- WatchList entity configuration ---
        modelBuilder.Entity<WatchList>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_WatchLists_UserId");

            // Unique constraint: a user can only watch an auction once
            entity.HasIndex(e => new { e.UserId, e.AuctionId }, "UQ_WatchLists_UserAuction").IsUnique();

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Auction).WithMany(p => p.WatchLists)
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WatchLists_Auction");

            entity.HasOne(d => d.User).WithMany(p => p.WatchLists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_WatchLists_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

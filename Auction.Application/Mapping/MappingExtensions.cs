using Auction.Application.DTOs.Auctions;
using Auction.Application.DTOs.Auth;
using Auction.Application.DTOs.Bids;
using Auction.Application.DTOs.Categories;
using Auction.Application.DTOs.Notifications;
using Auction.Application.DTOs.WatchList;
using Auction.Domain.Enums;

namespace Auction.Application.Mapping;

// Static mapping helpers — convert raw data into DTOs.
// We use manual mapping instead of AutoMapper to keep things
// explicit and easy to understand during code reviews.
public static class MappingExtensions
{
    // Convert a UserRole integer (stored in DB) to its string name
    public static string ToRoleString(int role) =>
        ((UserRole)role).ToString();

    // Convert an AuctionStatus integer to its string name
    public static string ToStatusString(int status) =>
        ((AuctionStatus)status).ToString();

    // Convert a NotificationType integer to its string name
    public static string ToNotificationTypeString(int type) =>
        ((NotificationType)type).ToString();

    // Build a UserProfileDto from raw user fields
    public static UserProfileDto ToUserProfileDto(
        int id, string fullName, string email, int role, DateTime createdAtUtc) =>
        new()
        {
            Id = id,
            FullName = fullName,
            Email = email,
            Role = ToRoleString(role),
            CreatedAtUtc = createdAtUtc
        };

    // Build an AuctionSummaryDto from raw auction fields
    public static AuctionSummaryDto ToAuctionSummaryDto(
        int id, string title, string categoryName, decimal startingPrice,
        decimal currentPrice, DateTime startTime, DateTime endTime,
        int status, string sellerName, string? thumbnailUrl) =>
        new()
        {
            Id = id,
            Title = title,
            CategoryName = categoryName,
            StartingPrice = startingPrice,
            CurrentPrice = currentPrice,
            StartTime = startTime,
            EndTime = endTime,
            Status = ToStatusString(status),
            SellerName = sellerName,
            ThumbnailUrl = thumbnailUrl
        };

    // Build a BidDto from raw bid fields
    public static BidDto ToBidDto(
        int id, int auctionId, string bidderName,
        decimal amount, DateTime bidTimeUtc, bool isWinningBid) =>
        new()
        {
            Id = id,
            AuctionId = auctionId,
            BidderName = bidderName,
            Amount = amount,
            BidTimeUtc = bidTimeUtc,
            IsWinningBid = isWinningBid
        };

    // Build a CategoryDto
    public static CategoryDto ToCategoryDto(int id, string name) =>
        new() { Id = id, Name = name };

    // Build a NotificationDto from raw notification fields
    public static NotificationDto ToNotificationDto(
        int id, int? auctionId, string message,
        int type, bool isRead, DateTime createdAtUtc) =>
        new()
        {
            Id = id,
            AuctionId = auctionId,
            Message = message,
            Type = ToNotificationTypeString(type),
            IsRead = isRead,
            CreatedAtUtc = createdAtUtc
        };

    // Build a WatchListItemDto
    public static WatchListItemDto ToWatchListItemDto(
        int id, int auctionId, string auctionTitle,
        decimal currentPrice, int auctionStatus,
        DateTime endTime, string? thumbnailUrl, DateTime createdAtUtc) =>
        new()
        {
            Id = id,
            AuctionId = auctionId,
            AuctionTitle = auctionTitle,
            CurrentPrice = currentPrice,
            AuctionStatus = ToStatusString(auctionStatus),
            EndTime = endTime,
            ThumbnailUrl = thumbnailUrl,
            CreatedAtUtc = createdAtUtc
        };
}

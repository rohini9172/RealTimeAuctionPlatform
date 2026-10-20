namespace Auction.Application.DTOs.Auctions;

// Generic paginated result wrapper used for auction list responses.
// T will be AuctionSummaryDto in practice.
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    // Calculated property — total number of pages
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

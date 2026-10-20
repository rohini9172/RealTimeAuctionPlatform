namespace Auction.Application.DTOs.Auctions;

// Query parameters for GET /api/auctions — supports search, filter, and pagination.
public class AuctionQueryParams
{
    // Free-text search against title
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }

    // Filter by status string: "Scheduled", "Live", "Ended", "Cancelled"
    public string? Status { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Pagination — defaults to page 1, 10 items per page
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

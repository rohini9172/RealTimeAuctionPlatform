using Auction.Application.DTOs.Auctions;
using Microsoft.AspNetCore.Http;

namespace Auction.Api.Models;

public class CreateAuctionFormRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal MinIncrement { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<IFormFile> Images { get; set; } = new();

    public CreateAuctionRequest ToCreateAuctionRequest(IEnumerable<string> imageUrls) => new()
    {
        CategoryId = CategoryId,
        Title = Title,
        Description = Description,
        StartingPrice = StartingPrice,
        MinIncrement = MinIncrement,
        StartTime = StartTime,
        EndTime = EndTime,
        ImageUrls = imageUrls.ToList()
    };
}

using Auction.Application.DTOs.Auctions;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Enums;
using Auction.Infrastructure.Persistence;
using Entities = Auction.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories;

// Data-access implementation for auctions.
// Projects entities directly into DTOs so the API never sees EF entities.
public class AuctionRepository : IAuctionRepository
{
    private readonly AuctionDbContext _dbContext;

    public AuctionRepository(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuctionDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbContext.Auctions
            .Where(a => a.Id == id)
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .Include(a => a.HighestBidder)
            .Include(a => a.Winner)
            .Include(a => a.AuctionImages)
            .Select(a => new AuctionDetailDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                StartingPrice = a.StartingPrice,
                MinIncrement = a.MinIncrement,
                CurrentPrice = a.CurrentPrice,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = ((AuctionStatus)a.Status).ToString(),
                SellerId = a.SellerId,
                SellerName = a.Seller.FullName,
                HighestBidderName = a.HighestBidder != null ? a.HighestBidder.FullName : null,
                WinnerName = a.Winner != null ? a.Winner.FullName : null,
                ImageUrls = a.AuctionImages.Select(i => i.ImageUrl).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<AuctionSummaryDto>> GetPagedAsync(AuctionQueryParams query, CancellationToken ct = default)
    {
        var baseQuery = _dbContext.Auctions
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .Include(a => a.AuctionImages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            baseQuery = baseQuery.Where(a => a.Title.Contains(query.Keyword));
        }

        if (query.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<AuctionStatus>(query.Status, ignoreCase: true, out var statusEnum))
        {
            baseQuery = baseQuery.Where(a => a.Status == (int)statusEnum);
        }

        if (query.MinPrice.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.CurrentPrice >= query.MinPrice.Value);
        }
        if (query.MaxPrice.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.CurrentPrice <= query.MaxPrice.Value);
        }

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(a => a.EndTime)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AuctionSummaryDto
            {
                Id = a.Id,
                Title = a.Title,
                CategoryName = a.Category.Name,
                StartingPrice = a.StartingPrice,
                CurrentPrice = a.CurrentPrice,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = ((AuctionStatus)a.Status).ToString(),
                SellerName = a.Seller.FullName,
                ThumbnailUrl = a.AuctionImages.FirstOrDefault() != null
                    ? a.AuctionImages.FirstOrDefault()!.ImageUrl
                    : null
            })
            .ToListAsync(ct);

        return new PagedResult<AuctionSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
    public async Task<List<AuctionSummaryDto>> GetBySellerAsync(int sellerId, CancellationToken ct = default)
    {
        return await _dbContext.Auctions
            .Where(a => a.SellerId == sellerId)
            .Include(a => a.Category)
            .Include(a => a.AuctionImages)
            .OrderByDescending(a => a.EndTime)
            .Select(a => new AuctionSummaryDto
            {
                Id = a.Id,
                Title = a.Title,
                CategoryName = a.Category.Name,
                StartingPrice = a.StartingPrice,
                CurrentPrice = a.CurrentPrice,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = ((AuctionStatus)a.Status).ToString(),
                SellerName = a.Seller.FullName,
                ThumbnailUrl = a.AuctionImages.FirstOrDefault() != null
                    ? a.AuctionImages.FirstOrDefault()!.ImageUrl
                    : null
            })
            .ToListAsync(ct);
    }

    public async Task<int> CreateAsync(int sellerId, CreateAuctionRequest request, CancellationToken ct = default)
    {
        var auction = new Entities.Auction
        {
            SellerId = sellerId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            StartingPrice = request.StartingPrice,
            MinIncrement = request.MinIncrement,
            CurrentPrice = request.StartingPrice,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = (int)AuctionStatus.Scheduled
        };

        _dbContext.Auctions.Add(auction);
        await _dbContext.SaveChangesAsync(ct);

        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            foreach (var url in request.ImageUrls)
            {
                _dbContext.AuctionImages.Add(new Entities.AuctionImage
                {
                    AuctionId = auction.Id,
                    ImageUrl = url
                });
            }
            await _dbContext.SaveChangesAsync(ct);
        }

        return auction.Id;
    }

    public async Task UpdateAsync(int id, UpdateAuctionRequest request, CancellationToken ct = default)
    {
        var auction = await _dbContext.Auctions.FindAsync(new object?[] { id }, ct);
        if (auction == null)
        {
            throw new Auction.Domain.Exceptions.NotFoundException("Auction not found.");
        }

        auction.CategoryId = request.CategoryId;
        auction.Title = request.Title;
        auction.Description = request.Description;
        auction.StartingPrice = request.StartingPrice;
        auction.MinIncrement = request.MinIncrement;
        auction.StartTime = request.StartTime;
        auction.EndTime = request.EndTime;

        var existingImages = _dbContext.AuctionImages.Where(i => i.AuctionId == id);
        _dbContext.AuctionImages.RemoveRange(existingImages);

        if (request.ImageUrls != null && request.ImageUrls.Any())
        {
            foreach (var url in request.ImageUrls)
            {
                _dbContext.AuctionImages.Add(new Entities.AuctionImage
                {
                    AuctionId = id,
                    ImageUrl = url
                });
            }
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        var auction = await _dbContext.Auctions.FindAsync(new object?[] { id }, ct);
        if (auction == null)
        {
            throw new Auction.Domain.Exceptions.NotFoundException("Auction not found.");
        }

        auction.Status = (int)AuctionStatus.Cancelled;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<int?> GetSellerIdAsync(int auctionId, CancellationToken ct = default)
    {
        return await _dbContext.Auctions
            .Where(a => a.Id == auctionId)
            .Select(a => (int?)a.SellerId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int?> GetStatusAsync(int auctionId, CancellationToken ct = default)
    {
        return await _dbContext.Auctions
            .Where(a => a.Id == auctionId)
            .Select(a => (int?)a.Status)
            .FirstOrDefaultAsync(ct);
    }
}
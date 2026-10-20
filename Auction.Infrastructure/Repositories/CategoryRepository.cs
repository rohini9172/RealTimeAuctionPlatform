using Auction.Application.DTOs.Categories;
using Auction.Application.Interfaces.Repositories;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories;

// Data-access implementation for categories.
public class CategoryRepository : ICategoryRepository
{
    private readonly AuctionDbContext _dbContext;

    public CategoryRepository(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync(ct);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbContext.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _dbContext.Categories.AnyAsync(c => c.Name == name, ct);
    }

    public async Task<int> CreateAsync(string name, CancellationToken ct = default)
    {
        var category = new Category { Name = name };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(ct);
        return category.Id;
    }
}
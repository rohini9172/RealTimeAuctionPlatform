using Auction.Application.DTOs.Categories;

namespace Auction.Application.Interfaces;

// Business operations for categories.
public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
}

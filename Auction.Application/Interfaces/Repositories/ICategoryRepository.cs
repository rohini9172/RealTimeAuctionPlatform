using Auction.Application.DTOs.Categories;

namespace Auction.Application.Interfaces.Repositories;

// Defines all category-related data operations.
public interface ICategoryRepository
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<int> CreateAsync(string name, CancellationToken ct = default);
}

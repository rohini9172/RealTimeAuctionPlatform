using Auction.Application.DTOs.Categories;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Enums;
using Auction.Domain.Exceptions;

namespace Auction.Infrastructure.Services;

// Business logic for categories.
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _categoryRepository.GetAllAsync(ct);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        // Prevent duplicate category names
        if (await _categoryRepository.ExistsByNameAsync(request.Name, ct))
        {
            throw new DomainException("A category with this name already exists.");
        }

        var id = await _categoryRepository.CreateAsync(request.Name, ct);
        return new CategoryDto { Id = id, Name = request.Name };
    }
}
using Auction.Application.DTOs.Categories;
using Auction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Controllers;

// Category endpoints.
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken ct = default)
    {
        var categories = await _categoryService.GetAllAsync(ct);
        return Ok(categories);
    }

    /// <summary>
    /// Create a new category (Admins only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct = default)
    {
        var category = await _categoryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }
}
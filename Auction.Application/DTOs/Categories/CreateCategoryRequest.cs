namespace Auction.Application.DTOs.Categories;

// Sent by an Admin when creating a new category.
public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

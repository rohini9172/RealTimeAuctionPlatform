namespace Auction.Application.DTOs.Categories;

// Returned in the category list.
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

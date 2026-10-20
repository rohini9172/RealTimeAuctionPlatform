namespace Auction.Application.DTOs.Auth;

// Returned by GET /api/auth/me — the currently logged-in user's profile.
public class UserProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

namespace Auction.Application.DTOs.Auth;

// Sent by the client when registering a new account.
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Only "Seller" or "Bidder" allowed from public registration.
    // Admin role is blocked in the validator.
    public string Role { get; set; } = string.Empty;
}

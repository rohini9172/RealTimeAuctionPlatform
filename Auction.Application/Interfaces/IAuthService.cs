using Auction.Application.DTOs.Auth;

namespace Auction.Application.Interfaces;

// Business operations for authentication.
// The Infrastructure layer implements this.
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task ResetPasswordByEmailAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<UserProfileDto> GetCurrentUserAsync(int userId, CancellationToken ct = default);
}

using Auction.Application.DTOs.Auth;

namespace Auction.Application.Interfaces.Repositories;

// Defines all user-related data operations.
// The Infrastructure layer implements this — Application only knows the contract.
public interface IUserRepository
{
    // Find a user by their email address (used during login and duplicate-email check)
    Task<UserProfileDto?> GetByEmailAsync(string email, CancellationToken ct = default);

    // Find a user by ID (used by GET /api/auth/me)
    Task<UserProfileDto?> GetByIdAsync(int id, CancellationToken ct = default);

    // Check if an email is already registered
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    // Create a new user — returns the new user's ID
    Task<int> CreateAsync(string fullName, string email, string passwordHash, int role, CancellationToken ct = default);

    // Get the stored password hash for login verification
    Task<string?> GetPasswordHashAsync(string email, CancellationToken ct = default);

    // Check if the user account is active
    Task<bool> IsActiveAsync(string email, CancellationToken ct = default);

    Task<bool> UpdatePasswordByEmailAsync(
        string email,
        string passwordHash,
        CancellationToken ct = default);
}

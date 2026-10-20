using Auction.Application.DTOs.Auth;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Enums;
using Auction.Infrastructure.Persistence;
using Auction.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auction.Infrastructure.Repositories;

// Data-access implementation for users.
// Application only knows the interface (IUserRepository); the concrete class lives here.
public class UserRepository : IUserRepository
{
    private readonly AuctionDbContext _dbContext;

    public UserRepository(AuctionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfileDto?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Where(u => u.Email == email)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = ((UserRole)u.Role).ToString(),
                CreatedAtUtc = u.CreatedAtUtc
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserProfileDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Where(u => u.Id == id)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = ((UserRole)u.Role).ToString(),
                CreatedAtUtc = u.CreatedAtUtc
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<int> CreateAsync(string fullName, string email, string passwordHash, int role, CancellationToken ct = default)
    {
        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);
        return user.Id;
    }

    public async Task<string?> GetPasswordHashAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Where(u => u.Email == email)
            .Select(u => u.PasswordHash)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsActiveAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Where(u => u.Email == email)
            .Select(u => u.IsActive)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdatePasswordByEmailAsync(
        string email,
        string passwordHash,
        CancellationToken ct = default)
    {
        var updated = await _dbContext.Users
            .Where(user => user.Email == email && user.IsActive)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(user => user.PasswordHash, passwordHash), ct);

        return updated == 1;
    }
}
using Auction.Application.DTOs.Auth;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Enums;
using Auction.Domain.Exceptions;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auction.Infrastructure.Services;

// Business logic for authentication: registration, login, JWT token generation.
// Infrastructure implements the IAuthService contract defined in Application.
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);

        // Prevent duplicate email
        if (await _userRepository.EmailExistsAsync(email, ct))
        {
            throw new DomainException("Email is already registered.");
        }

        // Hash the password with BCrypt (never store plain text)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Map role string to enum integer (only Seller or Bidder allowed by validator)
        var role = Enum.Parse<UserRole>(request.Role, true);

        var userId = await _userRepository.CreateAsync(
            request.FullName, email, passwordHash, (int)role, ct);

        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new DomainException("Failed to create user.");

        var token = GenerateToken(user.Id, user.Email, user.Role);
        var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        return new AuthResponse
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);

        // Look up stored password hash
        var passwordHash = await _userRepository.GetPasswordHashAsync(email, ct);
        if (passwordHash == null)
        {
            throw new DomainException("Invalid email or password.");
        }

        // Verify the supplied password against the hash
        if (!BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
        {
            throw new DomainException("Invalid email or password.");
        }

        // Reject inactive accounts
        var isActive = await _userRepository.IsActiveAsync(email, ct);
        if (!isActive)
        {
            throw new DomainException("Account is inactive. Please contact support.");
        }

        var user = await _userRepository.GetByEmailAsync(email, ct)
            ?? throw new DomainException("User not found.");

        var token = GenerateToken(user.Id, user.Email, user.Role);
        var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        return new AuthResponse
        {
            Token = token,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
    }

    public async Task<UserProfileDto> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");
        return user;
    }

    public async Task ResetPasswordByEmailAsync(
        ForgotPasswordRequest request,
        CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        if (!await _userRepository.UpdatePasswordByEmailAsync(email, passwordHash, ct))
        {
            throw new DomainException("No active account was found for that email address.");
        }
    }

    private string GenerateToken(int userId, string email, string role)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "RealTimeAuctionApi";
        var audience = _configuration["Jwt:Audience"] ?? "RealTimeAuctionClient";
        var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
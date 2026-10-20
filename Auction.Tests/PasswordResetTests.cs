using Auction.Application.DTOs.Auth;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Exceptions;
using Auction.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;

public class PasswordResetTests
{
    [Fact]
    public async Task ResetPasswordByEmail_UpdatesExistingAccountWithHashedPassword()
    {
        string? storedPasswordHash = null;
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.UpdatePasswordByEmailAsync(
                "user@example.com",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, passwordHash, _) =>
                storedPasswordHash = passwordHash)
            .ReturnsAsync(true);
        var service = new AuthService(users.Object, CreateConfiguration());

        await service.ResetPasswordByEmailAsync(new ForgotPasswordRequest
        {
            Email = "User@example.com",
            Password = "ValidPassword1"
        });

        Assert.NotEqual("ValidPassword1", storedPasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("ValidPassword1", storedPasswordHash));
    }

    [Fact]
    public async Task ResetPasswordByEmail_RejectsUnknownAccount()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(repository => repository.UpdatePasswordByEmailAsync(
                "missing@example.com",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var service = new AuthService(users.Object, CreateConfiguration());

        await Assert.ThrowsAsync<DomainException>(() =>
            service.ResetPasswordByEmailAsync(new ForgotPasswordRequest
            {
                Email = "Missing@example.com",
                Password = "ValidPassword1"
            }));
    }

    private static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection().Build();
}

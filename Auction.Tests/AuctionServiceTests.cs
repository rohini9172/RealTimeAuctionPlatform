using Auction.Application.DTOs.Auctions;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Exceptions;
using Auction.Infrastructure.Services;
using Moq;

public class AuctionServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsStartTimeInThePast()
    {
        var repository = new Mock<IAuctionRepository>();
        var service = new AuctionService(repository.Object);
        var request = new CreateAuctionRequest
        {
            CategoryId = 1,
            Title = "Camera",
            Description = "Used camera",
            StartingPrice = 100,
            MinIncrement = 5,
            StartTime = DateTime.UtcNow.AddMinutes(-1),
            EndTime = DateTime.UtcNow.AddHours(1)
        };

        await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(1, request));
        repository.Verify(
            x => x.CreateAsync(It.IsAny<int>(), It.IsAny<CreateAuctionRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RejectsAnotherSellersAuction()
    {
        var repository = new Mock<IAuctionRepository>();
        repository.Setup(x => x.GetSellerIdAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync(34);
        var service = new AuctionService(repository.Object);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateAsync(12, 56, new UpdateAuctionRequest()));

        repository.Verify(
            x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateAuctionRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
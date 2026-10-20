using Auction.Application.DTOs.Auctions;
using Auction.Application.Interfaces;
using Auction.Application.Interfaces.Repositories;
using Auction.Domain.Enums;
using Auction.Domain.Exceptions;

namespace Auction.Infrastructure.Services;

// Business logic for auctions: create, get, update, cancel, my-auctions.
// Ownership and state-transition rules live here, not in the controller.
public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;

    public AuctionService(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<AuctionDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Auction not found.");
        return auction;
    }

    public async Task<PagedResult<AuctionSummaryDto>> GetPagedAsync(AuctionQueryParams query, CancellationToken ct = default)
    {
        return await _auctionRepository.GetPagedAsync(query, ct);
    }

    public async Task<List<AuctionSummaryDto>> GetMyAuctionsAsync(int sellerId, CancellationToken ct = default)
    {
        return await _auctionRepository.GetBySellerAsync(sellerId, ct);
    }

    public async Task<AuctionDetailDto> CreateAsync(int sellerId, CreateAuctionRequest request, CancellationToken ct = default)
    {
        // Business rule: start time must be in the future, end time after start time
        if (request.StartTime <= DateTime.UtcNow)
        {
            throw new DomainException("Start time must be in the future.");
        }
        if (request.EndTime <= request.StartTime)
        {
            throw new DomainException("End time must be after start time.");
        }
        if (request.StartingPrice <= 0)
        {
            throw new DomainException("Starting price must be greater than zero.");
        }
        if (request.MinIncrement <= 0)
        {
            throw new DomainException("Minimum increment must be greater than zero.");
        }

        var id = await _auctionRepository.CreateAsync(sellerId, request, ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<AuctionDetailDto> UpdateAsync(int auctionId, int sellerId, UpdateAuctionRequest request, CancellationToken ct = default)
    {
        // Ownership check: only the seller can update
        var ownerId = await _auctionRepository.GetSellerIdAsync(auctionId, ct);
        if (ownerId == null)
        {
            throw new NotFoundException("Auction not found.");
        }
        if (ownerId != sellerId)
        {
            throw new ForbiddenException("You can only update your own auctions.");
        }

        // State check: only Scheduled auctions can be updated (no bids yet)
        var status = await _auctionRepository.GetStatusAsync(auctionId, ct);
        if (status == null)
        {
            throw new NotFoundException("Auction not found.");
        }
        if ((AuctionStatus)status.Value != AuctionStatus.Scheduled)
        {
            throw new DomainException("Auction cannot be updated after it has started or ended.");
        }

        await _auctionRepository.UpdateAsync(auctionId, request, ct);
        return await GetByIdAsync(auctionId, ct);
    }

    public async Task CancelAsync(int auctionId, int sellerId, CancellationToken ct = default)
    {
        var ownerId = await _auctionRepository.GetSellerIdAsync(auctionId, ct);
        if (ownerId == null)
        {
            throw new NotFoundException("Auction not found.");
        }
        if (ownerId != sellerId)
        {
            throw new ForbiddenException("You can only cancel your own auctions.");
        }

        var status = await _auctionRepository.GetStatusAsync(auctionId, ct);
        if (status == null)
        {
            throw new NotFoundException("Auction not found.");
        }
        if ((AuctionStatus)status.Value != AuctionStatus.Scheduled)
        {
            throw new DomainException("Only scheduled auctions can be cancelled.");
        }

        await _auctionRepository.CancelAsync(auctionId, ct);
    }
}
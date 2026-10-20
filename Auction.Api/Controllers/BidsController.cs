using Auction.Application.DTOs.Bids;
using Auction.Application.Interfaces;
using Auction.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Auction.Api.Controllers;

// Bidding endpoints.
[ApiController]
[Route("api/auctions/{auctionId}/bids")]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;
    private readonly IHubContext<AuctionHub> _hubContext;

    public BidsController(IBidService bidService, IHubContext<AuctionHub> hubContext)
    {
        _bidService = bidService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Place a bid on an auction (Bidders only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Bidder")]
    public async Task<IActionResult> PlaceBid(
        int auctionId,
        [FromBody] PlaceBidRequest request,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var bid = await _bidService.PlaceBidAsync(auctionId, userId, request, ct);
        await _hubContext.Clients
            .Group($"Auction_{auctionId}")
            .SendAsync("NewBid", bid, ct);
        return Ok(bid);
    }

    /// <summary>
    /// Get bid history for an auction.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuctionBids(
        int auctionId,
        CancellationToken ct = default)
    {
        var bids = await _bidService.GetAuctionBidsAsync(auctionId, ct);
        return Ok(bids);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                    User.FindFirst("sub");
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}

/// <summary>
/// Get the current user's bid history.
/// </summary>
[ApiController]
[Route("api/bids/my")]
public class MyBidsController : ControllerBase
{
    private readonly IBidService _bidService;

    public MyBidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [HttpGet]
    [Authorize(Roles = "Bidder")]
    public async Task<IActionResult> GetMyBids(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var bids = await _bidService.GetMyBidsAsync(userId, ct);
        return Ok(bids);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                    User.FindFirst("sub");
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
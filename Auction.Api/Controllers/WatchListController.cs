using Auction.Application.DTOs.WatchList;
using Auction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Controllers;

// Watchlist endpoints.
[ApiController]
[Route("api/watchlist")]
public class WatchListController : ControllerBase
{
    private readonly IWatchListService _watchListService;

    public WatchListController(IWatchListService watchListService)
    {
        _watchListService = watchListService;
    }

    /// <summary>
    /// Get the current user's watchlist.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyWatchList(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var items = await _watchListService.GetMyWatchListAsync(userId, ct);
        return Ok(items);
    }

    /// <summary>
    /// Add an auction to the current user's watchlist.
    /// </summary>
    [HttpPost("{auctionId}")]
    [Authorize]
    public async Task<IActionResult> AddToWatchList(
        int auctionId,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _watchListService.AddAsync(userId, auctionId, ct);
        return NoContent();
    }

    /// <summary>
    /// Remove an auction from the current user's watchlist.
    /// </summary>
    [HttpDelete("{auctionId}")]
    [Authorize]
    public async Task<IActionResult> RemoveFromWatchList(
        int auctionId,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _watchListService.RemoveAsync(userId, auctionId, ct);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                    User.FindFirst("sub");
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
using Auction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Auction.Api.Hubs;

// SignalR hub for real-time auction updates.
// Clients join auction-specific groups to receive price/bid updates.
[Authorize]
public class AuctionHub : Hub
{
    private readonly IAuctionService _auctionService;

    public AuctionHub(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    /// <summary>
    /// Join a group for a specific auction to receive real-time updates.
    /// </summary>
    public async Task JoinAuctionGroup(int auctionId)
    {
        if (auctionId <= 0)
        {
            throw new HubException("Auction ID must be positive.");
        }

        await _auctionService.GetByIdAsync(auctionId, Context.ConnectionAborted);
        var groupName = $"Auction_{auctionId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave an auction group.
    /// </summary>
    public async Task LeaveAuctionGroup(int auctionId)
    {
        var groupName = $"Auction_{auctionId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

}
using Auction.Application.DTOs.Notifications;
using Auction.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auction.Api.Controllers;

// Notification endpoints.
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get the current user's notifications.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetMyNotificationsAsync(userId, ct);
        return Ok(notifications);
    }

    /// <summary>
    /// Mark a notification as read.
    /// </summary>
    [HttpPatch("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(
        int id,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAsReadAsync(id, userId, ct);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                    User.FindFirst("sub");
        return claim != null ? int.Parse(claim.Value) : 0;
    }
}
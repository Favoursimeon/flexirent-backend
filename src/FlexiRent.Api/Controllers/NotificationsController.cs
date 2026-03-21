using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(
        INotificationService notificationService,
        ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? cursor = null)
    {
        var result = await _notificationService
            .GetNotificationsAsync(_currentUser.UserId, pageSize, cursor);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _notificationService.GetUnreadCountAsync(_currentUser.UserId);
        return Ok(new { count });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id, _currentUser.UserId);
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(_currentUser.UserId);
        return NoContent();
    }
}
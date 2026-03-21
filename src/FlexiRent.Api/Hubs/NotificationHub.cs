using FlexiRent.Infrastructure.Hubs;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlexiRent.Api.Hubs;

[Authorize]
public class NotificationHub : NotificationHubMarker
{
    private readonly ICurrentUserService _currentUser;

    public NotificationHub(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUser.UserId;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUser.UserId;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnDisconnectedAsync(exception);
    }
}
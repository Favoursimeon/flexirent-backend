using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlexiRent.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        // Send message to a booking group
        public async Task SendMessageToBooking(string bookingId, string message)
        {
            var claimUserId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var payload = new { BookingId = bookingId, SenderId = claimUserId, Body = message, SentAt = DateTime.UtcNow };
            await Clients.Group($"booking-{bookingId}").SendAsync("ReceiveBookingMessage", payload);
        }

        // Join booking group (rooms)
        public async Task JoinBooking(string bookingId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        }

        public async Task LeaveBooking(string bookingId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        }

        // Send direct message to user (targetUserId)
        public async Task SendDirectMessage(string targetUserId, string message)
        {
            var payload = new { SenderId = Context.UserIdentifier, TargetUserId = targetUserId, Body = message, SentAt = DateTime.UtcNow };
            await Clients.User(targetUserId).SendAsync("ReceiveDirectMessage", payload);
        }

        public override async Task OnConnectedAsync()
        {
            // Optionally set Context.UserIdentifier via claim mapping (by default it's NameIdentifier)
            await base.OnConnectedAsync();
        }
    }
}

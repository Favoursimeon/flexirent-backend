using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Persistence;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ChatHub(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public async Task JoinBookingRoom(string bookingId)
    {
        var userId = _currentUser.UserId;

        // Verify user belongs to this booking
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == Guid.Parse(bookingId) &&
                (b.UserId == userId || b.ProviderId == userId));

        if (booking is null)
        {
            await Clients.Caller.SendAsync("Error", "Not authorised for this booking.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
        await Clients.Caller.SendAsync("JoinedRoom", bookingId);
    }

    public async Task LeaveBookingRoom(string bookingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"booking-{bookingId}");
    }

    public async Task SendMessage(string bookingId, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return;

        var userId = _currentUser.UserId;
        var bookingGuid = Guid.Parse(bookingId);

        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == bookingGuid &&
                (b.UserId == userId || b.ProviderId == userId));

        if (booking is null)
        {
            await Clients.Caller.SendAsync("Error", "Not authorised for this booking.");
            return;
        }

        var message = new BookingMessage
        {
            Id = Guid.NewGuid(),
            BookingId = bookingGuid,
            SenderId = userId,
            Body = body.Trim(),
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        _db.BookingMessages.Add(message);
        await _db.SaveChangesAsync();

        await Clients.Group($"booking-{bookingId}").SendAsync("ReceiveMessage", new
        {
            id = message.Id,
            bookingId = message.BookingId,
            senderId = message.SenderId,
            body = message.Body,
            isRead = message.IsRead,
            sentAt = message.SentAt
        });
    }

    public async Task MarkMessagesRead(string bookingId)
    {
        var userId = _currentUser.UserId;
        var bookingGuid = Guid.Parse(bookingId);

        var unread = await _db.BookingMessages
            .Where(m =>
                m.BookingId == bookingGuid &&
                m.SenderId != userId &&
                !m.IsRead)
            .ToListAsync();

        foreach (var m in unread)
            m.IsRead = true;

        await _db.SaveChangesAsync();

        await Clients.Group($"booking-{bookingId}")
            .SendAsync("MessagesRead", new { bookingId, readBy = userId });
    }
}
using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FlexiRent.Infrastructure.Hubs;


namespace FlexiRent.Infrastructure.Services;

public interface INotificationService
{
    Task SendAsync(Guid userId, NotificationType type, string title, string message,
        string? actionUrl = null, Guid? referenceId = null);
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(Guid userId, int pageSize, Guid? cursor);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationHubMarker> _hubContext;
    private readonly IEmailService _emailService;

    public NotificationService(
        AppDbContext db,
        IHubContext<NotificationHubMarker> hubContext,
        IEmailService emailService)
    {
        _db = db;
        _hubContext = hubContext;
        _emailService = emailService;
    }

    public async Task SendAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        Guid? referenceId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // Push real-time via SignalR
        await _hubContext.Clients
            .Group($"user-{userId}")
            .SendAsync("ReceiveNotification", new
            {
                id = notification.Id,
                type = notification.Type.ToString(),
                title = notification.Title,
                message = notification.Message,
                actionUrl = notification.ActionUrl,
                isRead = notification.IsRead,
                createdAt = notification.CreatedAt
            });
    }

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(
        Guid userId, int pageSize, Guid? cursor)
    {
        var query = _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorDate = await _db.Notifications
                .Where(n => n.Id == cursor.Value)
                .Select(n => n.CreatedAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(n => n.CreatedAt < cursorDate);
        }

        var total = await query.CountAsync();
        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<NotificationDto>
        {
            Items = items.Select(MapToDto).ToList(),
            NextCursor = hasMore ? items.Last().Id : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new ApplicationException("Notification not found.");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Type = n.Type,
        Title = n.Title,
        Message = n.Message,
        IsRead = n.IsRead,
        ActionUrl = n.ActionUrl,
        ReferenceId = n.ReferenceId,
        CreatedAt = n.CreatedAt,
        ReadAt = n.ReadAt
    };
}
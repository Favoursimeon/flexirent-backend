using FlexiRent.Domain.Enums;

namespace FlexiRent.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
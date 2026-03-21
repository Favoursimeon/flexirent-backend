using FlexiRent.Domain.Enums;

namespace FlexiRent.Domain.Entities;

public class ViewingSchedule
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public ViewingStatus Status { get; set; } = ViewingStatus.Pending;
    public DateTime ScheduledAt { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Property Property { get; set; } = null!;
}
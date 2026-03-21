using FlexiRent.Domain.Enums;

namespace FlexiRent.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid PropertyId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public User Provider { get; set; } = null!;
    public Property Property { get; set; } = null!;
    public ICollection<BookingMessage> Messages { get; set; } = new List<BookingMessage>();
}
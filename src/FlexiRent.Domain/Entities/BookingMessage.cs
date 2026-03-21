namespace FlexiRent.Domain.Entities;

public class BookingMessage
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid SenderId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Booking Booking { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
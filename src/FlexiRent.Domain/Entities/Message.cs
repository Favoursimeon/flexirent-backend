namespace FlexiRent.Domain.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string Body { get; set; } = default!;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}

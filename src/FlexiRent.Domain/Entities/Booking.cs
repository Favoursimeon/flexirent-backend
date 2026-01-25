namespace FlexiRent.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = default!;
}

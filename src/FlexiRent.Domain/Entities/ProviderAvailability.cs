namespace FlexiRent.Domain.Entities;

public class ProviderAvailability
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User Provider { get; set; } = null!;
}
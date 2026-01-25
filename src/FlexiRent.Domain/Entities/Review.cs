namespace FlexiRent.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public string TargetType { get; set; } = default!;
    public Guid TargetId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
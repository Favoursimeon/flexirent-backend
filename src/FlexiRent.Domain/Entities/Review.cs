namespace FlexiRent.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User Author { get; set; } = null!;
}
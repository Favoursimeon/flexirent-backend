namespace FlexiRent.Domain.Entities;

public class PropertyImage
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Property Property { get; set; } = null!;
}
namespace FlexiRent.Domain.Entities;

public class Property
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal PricePerMonth { get; set; }
    public bool IsAvailable { get; set; }
}

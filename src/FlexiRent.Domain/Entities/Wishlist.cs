namespace FlexiRent.Domain.Entities;

public class Wishlist
{
    public Guid UserId { get; set; }
    public Guid TargetId { get; set; }
}
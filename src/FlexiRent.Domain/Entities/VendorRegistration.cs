namespace FlexiRent.Domain.Entities;

public class VendorRegistration
{
    public Guid Id { get; set; }
    public string BusinessName { get; set; } = default!;
    public string Details { get; set; } = default!;
}

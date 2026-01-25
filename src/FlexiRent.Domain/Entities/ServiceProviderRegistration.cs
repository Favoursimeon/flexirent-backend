namespace FlexiRent.Domain.Entities;

public class ServiceProviderRegistration
{
    public Guid Id { get; set; }
    public Guid ApplicantUserId { get; set; }
    public string ServiceType { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Details { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

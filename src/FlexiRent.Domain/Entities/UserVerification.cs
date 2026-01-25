namespace FlexiRent.Domain.Entities;

public class UserVerification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string VerificationToken { get; set; } = default!;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

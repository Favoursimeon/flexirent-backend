namespace FlexiRent.Domain.Entities;

public class UserVerification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string VerificationToken { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public bool IsVerified { get; set; }
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
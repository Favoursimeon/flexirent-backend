namespace FlexiRent.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public bool MfaEnabled { get; set; }
    public string? MfaSecretKey { get; set; }
    public string? GoogleId { get; set; }
    public string? GoogleEmail { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public string[]? MfaBackupCodes { get; set; }

    public Profile? Profile { get; set; }
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserVerification> Verifications { get; set; } = new List<UserVerification>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Booking> ProvidedBookings { get; set; } = new List<Booking>();
    public ICollection<ViewingSchedule> ViewingSchedules { get; set; } = new List<ViewingSchedule>();
    public ICollection<ProviderAvailability> Availabilities { get; set; } = new List<ProviderAvailability>();
    public ICollection<BookingMessage> SentMessages { get; set; } = new List<BookingMessage>();
    public ICollection<RentalLease> Leases { get; set; } = new List<RentalLease>();
    public ICollection<RentalPayment> Payments { get; set; } = new List<RentalPayment>();
    public ICollection<PaymentAccount> PaymentAccounts { get; set; } = new List<PaymentAccount>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
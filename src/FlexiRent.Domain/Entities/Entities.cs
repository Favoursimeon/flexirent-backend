namespace FlexiRent.Domain.Entities;

public class Profile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int FlexiScore { get; set; } = 0;

    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    // Privacy
    public bool ShowOnlineStatus { get; set; } = true;
    public bool IsSearchable { get; set; } = true;

    // Verification
    public bool IsVerified { get; set; }
    public string? VerificationDocumentUrl { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;


    // Preferences
    public bool PropertyAlerts { get; set; } = true;
    public string? PreferredRegion { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }

    // Soft delete / account deletion
    public bool DeletionRequested { get; set; }
    public string? DeletionReason { get; set; }
    public DateTime? DeletionRequestedAt { get; set; }
}
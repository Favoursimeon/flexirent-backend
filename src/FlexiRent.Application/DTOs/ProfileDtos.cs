namespace FlexiRent.Application.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsVerified { get; set; }
    public bool ShowOnlineStatus { get; set; }
    public bool IsSearchable { get; set; }
}

public class UpdateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class EmergencyContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
}

public class PrivacySettingsDto
{
    public bool ShowOnlineStatus { get; set; }
    public bool IsSearchable { get; set; }
}

public class UserPreferencesDto
{
    public bool PropertyAlerts { get; set; }
    public string? PreferredRegion { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
}

public class DeleteAccountRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

public class AvatarUploadResultDto
{
    public string AvatarUrl { get; set; } = string.Empty;
}
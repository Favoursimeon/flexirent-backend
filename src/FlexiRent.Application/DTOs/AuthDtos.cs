namespace FlexiRent.Application.DTOs;

public record EnableMfaResponseDto(
    string SecretKey, 
    string QrCodeUri, 
    string[] BackupCodes);
public record VerifyTotpDto(
    string Code);
public record LinkGoogleDto(
    string IdToken);
public record SecuritySettingsDto(
    bool MfaEnabled, 
    bool GoogleLinked,
    string? GoogleEmail, 
    DateTime? LastPasswordChange);
public record UpdateSecuritySettingsDto(
    string? CurrentPassword, 
    string? NewPassword);
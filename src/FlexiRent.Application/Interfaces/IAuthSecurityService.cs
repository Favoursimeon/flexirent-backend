using FlexiRent.Application.DTOs;

namespace FlexiRent.Application.Interfaces;

public interface IAuthSecurityService
{
    Task<EnableMfaResponseDto> EnableMfaAsync(Guid userId);
    Task<bool> VerifyMfaAsync(Guid userId, VerifyTotpDto dto);
    Task DisableMfaAsync(Guid userId, VerifyTotpDto dto);
    Task<SecuritySettingsDto> GetSecuritySettingsAsync(Guid userId);
    Task UpdateSecuritySettingsAsync(Guid userId, UpdateSecuritySettingsDto dto);
    Task<string> LinkGoogleAsync(Guid userId, LinkGoogleDto dto);
    Task UnlinkGoogleAsync(Guid userId);
}
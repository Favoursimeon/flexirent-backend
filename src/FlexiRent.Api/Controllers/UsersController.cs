using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IProfileService profileService, ICurrentUserService currentUser)
    {
        _profileService = profileService;
        _currentUser = currentUser;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _profileService.GetProfileAsync(_currentUser.UserId);
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var result = await _profileService.UpdateProfileAsync(_currentUser.UserId, dto);
        return Ok(result);
    }

    [HttpPut("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        var result = await _profileService.UploadAvatarAsync(_currentUser.UserId, file);
        return Ok(result);
    }

    [HttpPut("emergency-contact")]
    public async Task<IActionResult> UpdateEmergencyContact([FromBody] EmergencyContactDto dto)
    {
        await _profileService.UpdateEmergencyContactAsync(_currentUser.UserId, dto);
        return NoContent();
    }

    [HttpPost("verification")]
    public async Task<IActionResult> SubmitVerification(IFormFile document)
    {
        // Handled in verification service — placeholder for now
        return Ok(new { message = "Verification submission received." });
    }

    [HttpPut("privacy")]
    public async Task<IActionResult> UpdatePrivacy([FromBody] PrivacySettingsDto dto)
    {
        await _profileService.UpdatePrivacySettingsAsync(_currentUser.UserId, dto);
        return NoContent();
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UserPreferencesDto dto)
    {
        await _profileService.UpdatePreferencesAsync(_currentUser.UserId, dto);
        return NoContent();
    }

    [HttpPost("delete-request")]
    public async Task<IActionResult> RequestDeletion([FromBody] DeleteAccountRequestDto dto)
    {
        await _profileService.RequestAccountDeletionAsync(_currentUser.UserId, dto);
        return Ok(new { message = "Account deletion request submitted." });
    }
}
using FlexiRent.Application.DTOs;
using FlexiRent.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlexiRent.API.Controllers;

[ApiController]
[Route("api/security")]
[Authorize]
public class SecuritySettingsController : ControllerBase
{
    private readonly IAuthSecurityService _security;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public SecuritySettingsController(IAuthSecurityService security) => _security = security;

    [HttpGet]
    public async Task<IActionResult> GetSettings() =>
        Ok(await _security.GetSecuritySettingsAsync(UserId));

    [HttpPut]
    public async Task<IActionResult> UpdateSettings(UpdateSecuritySettingsDto dto)
    {
        await _security.UpdateSecuritySettingsAsync(UserId, dto);
        return NoContent();
    }

    [HttpPost("mfa/enable")]
    public async Task<IActionResult> EnableMfa() =>
        Ok(await _security.EnableMfaAsync(UserId));

    [HttpPost("mfa/verify")]
    public async Task<IActionResult> VerifyMfa(VerifyTotpDto dto) =>
        Ok(new { verified = await _security.VerifyMfaAsync(UserId, dto) });

    [HttpPost("mfa/disable")]
    public async Task<IActionResult> DisableMfa(VerifyTotpDto dto)
    {
        await _security.DisableMfaAsync(UserId, dto);
        return NoContent();
    }

    [HttpPost("oauth/google/link")]
    public async Task<IActionResult> LinkGoogle(LinkGoogleDto dto) =>
        Ok(new { linkedEmail = await _security.LinkGoogleAsync(UserId, dto) });

    [HttpDelete("oauth/google/unlink")]
    public async Task<IActionResult> UnlinkGoogle()
    {
        await _security.UnlinkGoogleAsync(UserId);
        return NoContent();
    }
}
using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _authService.RegisterAsync(req);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _authService.LoginAsync(req);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var result = await _authService.RefreshAsync(req.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        await _authService.LogoutAsync(req.RefreshToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetRequestDto req)
    {
        await _authService.RequestPasswordResetAsync(req);
        // Always return 200 — never reveal if email exists
        return Ok(new { message = "If that email is registered, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetConfirmDto req)
    {
        await _authService.ConfirmPasswordResetAsync(req);
        return Ok(new { message = "Password has been reset successfully. Please log in." });
    }
}
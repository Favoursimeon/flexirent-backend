using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) { _auth = auth; }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var resp = await _auth.RegisterAsync(req);
            return Ok(resp);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var resp = await _auth.LoginAsync(req);
            return Ok(resp);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
        {
            var resp = await _auth.RefreshAsync(req.RefreshToken);
            return Ok(resp);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
        {
            await _auth.LogoutAsync(req.RefreshToken);
            return NoContent();
        }

        [HttpPost("resend-verification")]
        public IActionResult ResendVerification([FromBody] Dictionary<string,string> data)
        {
            // Implement resend flow via IEmailService; stubbed
            return Ok(new { message = "verification resent (stub)" });
        }

        [HttpGet("verify-email/{token}")]
        public async Task<IActionResult> VerifyEmail(string token, [FromServices] AppDbContext db)
        {
            var v = await db.UserVerifications.FirstOrDefaultAsync(x => x.VerificationToken == token);
            if (v == null) return NotFound();
            v.IsVerified = true;
            v.VerifiedAt = DateTime.UtcNow;
            var user = await db.Users.FindAsync(v.UserId);
            if (user != null) user.EmailConfirmed = true;
            await db.SaveChangesAsync();
            return Ok(new { message = "email verified" });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] Dictionary<string, string> body)
        {
            // send reset token via email
            return Ok(new { message = "reset email sent (stub)" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Dictionary<string,string> body, [FromServices] AppDbContext db)
        {
            // implement reset using token
            return Ok(new { message = "password reset (stub)" });
        }
    }
}

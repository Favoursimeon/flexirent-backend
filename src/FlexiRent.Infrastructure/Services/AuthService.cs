using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace FlexiRent.Infrastructure.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req);
    Task<AuthResponse> LoginAsync(LoginRequest req);
    Task<AuthResponse> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task RequestPasswordResetAsync(PasswordResetRequestDto req);
    Task ConfirmPasswordResetAsync(PasswordResetConfirmDto req);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext db, IJwtService jwtService, IEmailService emailService)
    {
        _db = db;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            throw new ApplicationException("Email already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            EmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);

        _db.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Role = req.Role,
            AssignedAt = DateTime.UtcNow
        });

        _db.Profiles.Add(new Profile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = req.FirstName,
            LastName = req.LastName,
            CreatedAt = DateTime.UtcNow
        });

        var verification = new UserVerification
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            VerificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        _db.UserVerifications.Add(verification);
        await _db.SaveChangesAsync();

        await _emailService.SendEmailAsync(
            user.Email,
            "Verify your FlexiRent account",
            $"Your verification token: {verification.VerificationToken}\n\nThis token expires in 24 hours."
        );

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new ApplicationException("Invalid email or password.");

        if (!user.IsActive)
            throw new ApplicationException("This account has been deactivated.");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var tokenEntity = await _jwtService.GetValidRefreshTokenAsync(refreshToken);

        if (tokenEntity is null)
            throw new ApplicationException("Invalid or expired refresh token. Please log in again.");

        if (!tokenEntity.User.IsActive)
            throw new ApplicationException("This account has been deactivated.");

        await _jwtService.RevokeRefreshTokenAsync(refreshToken);

        return await BuildAuthResponseAsync(tokenEntity.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _jwtService.RevokeRefreshTokenAsync(refreshToken);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var (accessToken, expiresAt) = await _jwtService.GenerateAccessTokenAsync(user);
        var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task RequestPasswordResetAsync(PasswordResetRequestDto req)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == req.Email);

        // Always return success — never reveal whether email exists
        if (user is null || !user.IsActive) return;

        // Invalidate any existing unused tokens for this user
        var existingTokens = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var t in existingTokens)
            t.IsUsed = true;

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            IsUsed = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };

        _db.PasswordResetTokens.Add(resetToken);
        await _db.SaveChangesAsync();

        await _emailService.SendEmailAsync(
            user.Email,
            "Reset your FlexiRent password",
            $"Use the following token to reset your password:\n\n{resetToken.Token}\n\nThis token expires in 30 minutes.\n\nIf you did not request a password reset, please ignore this email."
        );
    }

    public async Task ConfirmPasswordResetAsync(PasswordResetConfirmDto req)
    {
        if (req.NewPassword != req.ConfirmPassword)
            throw new ApplicationException("Passwords do not match.");

        var tokenEntity = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == req.Token && !t.IsUsed);

        if (tokenEntity is null)
            throw new ApplicationException("Invalid or already used reset token.");

        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            tokenEntity.IsUsed = true;
            await _db.SaveChangesAsync();
            throw new ApplicationException("Reset token has expired. Please request a new one.");
        }

        // Update password and mark token used
        tokenEntity.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        tokenEntity.User.UpdatedAt = DateTime.UtcNow;
        tokenEntity.IsUsed = true;

        // Revoke all active refresh tokens — force re-login everywhere
        var activeRefreshTokens = await _db.RefreshTokens
            .Where(t => t.UserId == tokenEntity.UserId && !t.Revoked)
            .ToListAsync();

        foreach (var t in activeRefreshTokens)
            t.Revoked = true;

        await _db.SaveChangesAsync();
    }


}
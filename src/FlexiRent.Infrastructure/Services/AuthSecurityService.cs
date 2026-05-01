using FlexiRent.Application.DTOs;
using FlexiRent.Application.Interfaces;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OtpNet;
using QRCoder;

namespace FlexiRent.Infrastructure.Services;

public class AuthSecurityService : IAuthSecurityService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthSecurityService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<EnableMfaResponseDto> EnableMfaAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");

        var secretKey = KeyGeneration.GenerateRandomKey(20);
        var base32Secret = Base32Encoding.ToString(secretKey);

        var backupCodes = Enumerable.Range(0, 8)
            .Select(_ => Guid.NewGuid().ToString("N")[..8].ToUpper())
            .ToArray();

        var appName = _config["App:Name"] ?? "FlexiRent";
        var qrUri = $"otpauth://totp/{Uri.EscapeDataString(appName)}:{Uri.EscapeDataString(user.Email)}" +
                    $"?secret={base32Secret}&issuer={Uri.EscapeDataString(appName)}";

        user.MfaSecretKey = base32Secret;
        user.MfaBackupCodes = backupCodes;
        await _db.SaveChangesAsync();

        return new EnableMfaResponseDto(base32Secret, qrUri, backupCodes);
    }

    public async Task<bool> VerifyMfaAsync(Guid userId, VerifyTotpDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");
        if (string.IsNullOrEmpty(user.MfaSecretKey))
            throw new ApplicationException("MFA not set up.");

        var secretKey = Base32Encoding.ToBytes(user.MfaSecretKey);
        var totp = new Totp(secretKey);
        var valid = totp.VerifyTotp(dto.Code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        if (valid && !user.MfaEnabled)
        {
            user.MfaEnabled = true;
            await _db.SaveChangesAsync();
        }
        return valid;
    }

    public async Task DisableMfaAsync(Guid userId, VerifyTotpDto dto)
    {
        var verified = await VerifyMfaAsync(userId, dto);
        if (!verified) throw new ApplicationException("Invalid TOTP code.");
        var user = await _db.Users.FindAsync(userId)!;
        user!.MfaEnabled = false;
        user.MfaSecretKey = null;
        user.MfaBackupCodes = null;
        await _db.SaveChangesAsync();
    }

    public async Task<SecuritySettingsDto> GetSecuritySettingsAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");
        return new SecuritySettingsDto(
            user.MfaEnabled,
            user.GoogleId != null,
            user.GoogleEmail,
            user.LastPasswordChange);
    }

    public async Task UpdateSecuritySettingsAsync(Guid userId, UpdateSecuritySettingsDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            if (string.IsNullOrEmpty(dto.CurrentPassword))
                throw new ApplicationException("Current password required.");
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new ApplicationException("Current password is incorrect.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.LastPasswordChange = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> LinkGoogleAsync(Guid userId, LinkGoogleDto dto)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientId"] }
            });

        var existing = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);
        if (existing != null && existing.Id != userId)
            throw new ApplicationException("This Google account is already linked to another user.");

        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");
        user.GoogleId = payload.Subject;
        user.GoogleEmail = payload.Email;
        await _db.SaveChangesAsync();
        return payload.Email;
    }

    public async Task UnlinkGoogleAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new ApplicationException("User not found.");
        user.GoogleId = null;
        user.GoogleEmail = null;
        await _db.SaveChangesAsync();
    }
}
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FlexiRent.Infrastructure.Services;

public interface IJwtService
{
    Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync(Guid userId);
    ClaimsPrincipal? ValidateAccessToken(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task<RefreshToken?> GetValidRefreshTokenAsync(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public JwtService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<(string token, DateTime expiresAt)> GenerateAccessTokenAsync(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");

        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _db.UserRoles
            .Where(r => r.UserId == user.Id)
            .Select(r => r.Role.ToString())
            .ToListAsync();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("email_verified", user.EmailConfirmed.ToString().ToLower())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var expiryMinutes = double.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "15");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured."),
            audience: jwtSettings["Audience"]
                ?? throw new InvalidOperationException("JwtSettings:Audience is not configured."),
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var expiryDays = double.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

        var refresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            Revoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync();

        return refresh.Token;
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<RefreshToken?> GetValidRefreshTokenAsync(string token)
    {
        return await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == token &&
                !t.Revoked &&
                t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var tokenEntity = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token && !t.Revoked);

        if (tokenEntity is null) return false;

        tokenEntity.Revoked = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
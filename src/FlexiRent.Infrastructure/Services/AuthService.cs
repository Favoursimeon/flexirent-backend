using AutoMapper;
using BCrypt.Net;
using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FlexiRent.Infrastructure.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest req);
        Task<AuthResponse> LoginAsync(LoginRequest req);
        Task<AuthResponse> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
    }

    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _users;
        private readonly IGenericRepository<RefreshToken> _refreshTokens;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(IGenericRepository<User> users,
            IGenericRepository<RefreshToken> refreshTokens,
            ApplicationDbContext db,
            IConfiguration config,
            IEmailService emailService)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _db = db;
            _config = config;
            _emailService = emailService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
        {
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                throw new ApplicationException("Email already in use");

            var user = new User
            {
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // assign role
            var role = new UserRole { UserId = user.Id, Role = req.Role };
            _db.UserRoles.Add(role);
            _db.Profiles.Add(new Profile { UserId = user.Id, FullName = req.FullName });
            // create verification token
            var verification = new UserVerification
            {
                UserId = user.Id,
                VerificationToken = Guid.NewGuid().ToString()
            };
            _db.UserVerifications.Add(verification);
            await _db.SaveChangesAsync();

            // Send email (SendGrid)
            await _emailService.SendEmailAsync(user.Email, "Verify your account",
                $"Use token: {verification.VerificationToken}");

            return await GenerateTokensForUserAsync(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest req)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                throw new ApplicationException("Invalid credentials");

            return await GenerateTokensForUserAsync(user);
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken)
        {
            var tokenEntity = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken && !t.Revoked);
            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
                throw new ApplicationException("Invalid refresh token");

            var user = await _db.Users.FindAsync(tokenEntity.UserId);
            if (user == null) throw new ApplicationException("User not found");

            // Revoke old refresh token and issue new
            tokenEntity.Revoked = true;
            await _db.SaveChangesAsync();

            return await GenerateTokensForUserAsync(user);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var tokenEntity = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
            if (tokenEntity != null)
            {
                tokenEntity.Revoked = true;
                await _db.SaveChangesAsync();
            }
        }

        private async Task<AuthResponse> GenerateTokensForUserAsync(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _db.UserRoles.Where(r => r.UserId == user.Id).Select(r => r.Role).ToListAsync();
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpiryMinutes"])),
                signingCredentials: creds
            );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Create refresh token
            var refresh = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["RefreshTokenExpiryDays"]))
            };
            _db.RefreshTokens.Add(refresh);
            await _db.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = encodedToken,
                RefreshToken = refresh.Token,
                ExpiresAt = token.ValidTo
            };
        }
    }
}

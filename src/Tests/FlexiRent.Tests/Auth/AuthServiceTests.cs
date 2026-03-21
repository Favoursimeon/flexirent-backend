using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Persistence;
using FlexiRent.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using Moq;

namespace FlexiRent.Tests.Auth;

public class AuthServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly IAuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _jwtServiceMock = new Mock<IJwtService>();
        _emailServiceMock = new Mock<IEmailService>();

        // Default JWT mock behaviour
        _jwtServiceMock
        .Setup(j => j.GenerateAccessTokenAsync(It.IsAny<User>()))
        .ReturnsAsync(("mock-access-token", DateTime.UtcNow.AddMinutes(15)));

        _jwtServiceMock
            .Setup(j => j.GenerateRefreshTokenAsync(It.IsAny<Guid>()))
            .ReturnsAsync("mock-refresh-token");

        _jwtServiceMock
            .Setup(j => j.RevokeRefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _authService = new AuthService(_db, _jwtServiceMock.Object, _emailServiceMock.Object);
    }

    // ── Registration ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        var req = new RegisterRequest
        {
            Email = "test@flexirent.com",
            Password = "Password1!",
            FirstName = "Test",
            LastName = "User",
            Role = AppRole.User
        };

        var result = await _authService.RegisterAsync(req);

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock-access-token");
        result.RefreshToken.Should().Be("mock-refresh-token");
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserInDatabase()
    {
        var req = new RegisterRequest
        {
            Email = "newuser@flexirent.com",
            Password = "Password1!",
            FirstName = "New",
            LastName = "User",
            Role = AppRole.User
        };

        await _authService.RegisterAsync(req);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
        user.EmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_CreatesProfileAndRole()
    {
        var req = new RegisterRequest
        {
            Email = "profile@flexirent.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Doe",
            Role = AppRole.User
        };

        await _authService.RegisterAsync(req);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == user!.Id);
        var role = await _db.UserRoles.FirstOrDefaultAsync(r => r.UserId == user!.Id);

        profile.Should().NotBeNull();
        profile!.FirstName.Should().Be("John");
        profile.LastName.Should().Be("Doe");
        role.Should().NotBeNull();
        role!.Role.Should().Be(AppRole.User);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsApplicationException()
    {
        var req = new RegisterRequest
        {
            Email = "duplicate@flexirent.com",
            Password = "Password1!",
            FirstName = "First",
            LastName = "User",
            Role = AppRole.User
        };

        await _authService.RegisterAsync(req);

        var act = async () => await _authService.RegisterAsync(req);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Email already in use.");
    }

    [Fact]
    public async Task RegisterAsync_SendsVerificationEmail()
    {
        var req = new RegisterRequest
        {
            Email = "verify@flexirent.com",
            Password = "Password1!",
            FirstName = "Test",
            LastName = "User",
            Role = AppRole.User
        };

        await _authService.RegisterAsync(req);

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(
                "verify@flexirent.com",
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        await SeedUserAsync("login@flexirent.com", "Password1!");

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "login@flexirent.com",
            Password = "Password1!"
        });

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock-access-token");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsApplicationException()
    {
        await SeedUserAsync("wrong@flexirent.com", "Password1!");

        var act = async () => await _authService.LoginAsync(new LoginRequest
        {
            Email = "wrong@flexirent.com",
            Password = "WrongPassword!"
        });

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ThrowsApplicationException()
    {
        var act = async () => await _authService.LoginAsync(new LoginRequest
        {
            Email = "nobody@flexirent.com",
            Password = "Password1!"
        });

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedAccount_ThrowsApplicationException()
    {
        await SeedUserAsync("inactive@flexirent.com", "Password1!", isActive: false);

        var act = async () => await _authService.LoginAsync(new LoginRequest
        {
            Email = "inactive@flexirent.com",
            Password = "Password1!"
        });

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("This account has been deactivated.");
    }

    // ── Token refresh ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshAsync_WithValidToken_ReturnsNewAuthResponse()
    {
        var user = await SeedUserAsync("refresh@flexirent.com", "Password1!");
        var validToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Revoked = false,
            User = user,
            CreatedAt = DateTime.UtcNow
        };

        _jwtServiceMock
            .Setup(j => j.GetValidRefreshTokenAsync("valid-refresh-token"))
            .ReturnsAsync(validToken);

        var result = await _authService.RefreshAsync("valid-refresh-token");

        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock-access-token");
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_ThrowsApplicationException()
    {
        _jwtServiceMock
            .Setup(j => j.GetValidRefreshTokenAsync("expired-token"))
            .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _authService.RefreshAsync("expired-token");

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Invalid or expired refresh token. Please log in again.");
    }

    [Fact]
    public async Task RefreshAsync_WithRevokedToken_ThrowsApplicationException()
    {
        _jwtServiceMock
            .Setup(j => j.GetValidRefreshTokenAsync("revoked-token"))
            .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _authService.RefreshAsync("revoked-token");

        await act.Should().ThrowAsync<ApplicationException>();
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_RevokesRefreshToken()
    {
        var user = await SeedUserAsync("logout@flexirent.com", "Password1!");
        var tokenValue = "logout-refresh-token";

        _jwtServiceMock
            .Setup(j => j.RevokeRefreshTokenAsync(tokenValue))
            .ReturnsAsync(true);

        await _authService.LogoutAsync(tokenValue);

        _jwtServiceMock.Verify(
            j => j.RevokeRefreshTokenAsync(tokenValue),
            Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<User> SeedUserAsync(
        string email,
        string password,
        bool isActive = true)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            EmailConfirmed = true,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    private async Task<RefreshToken> SeedRefreshTokenAsync(
        Guid userId,
        DateTime? expiresAt = null,
        bool revoked = false)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
            Revoked = revoked,
            CreatedAt = DateTime.UtcNow
        };

        _db.RefreshTokens.Add(token);
        await _db.SaveChangesAsync();
        return token;
    }
}
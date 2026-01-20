using System.Threading.Tasks;
using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class AuthServiceIntegrationTests
{
    private ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "FlexiRentTestDb_" + System.Guid.NewGuid())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RegisterAndLogin_ShouldReturnTokens()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var configDict = new Dictionary<string, string>
        {
            {"JwtSettings:Secret", "TestSecretForUnitTestsWhichIsLongEnough"},
            {"JwtSettings:Issuer", "FlexiRent"},
            {"JwtSettings:Audience", "FlexiRentClients"},
            {"JwtSettings:AccessTokenExpiryMinutes", "60"},
            {"JwtSettings:RefreshTokenExpiryDays", "30"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();
        var emailMock = new Mock<FlexiRent.Infrastructure.Services.IEmailService>();
        var usersRepository = new FlexiRent.Infrastructure.Repositories.GenericRepository<User>(db);
        var refreshRepo = new FlexiRent.Infrastructure.Repositories.GenericRepository<RefreshToken>(db);
        var authService = new AuthService(usersRepository, refreshRepo, db, config, emailMock.Object);

        var register = new RegisterRequest { Email = "testuser@example.com", Password = "P@ssw0rd", FullName = "Test User", Role = "Client" };

        // Act
        var resp = await authService.RegisterAsync(register);

        // Assert registration returns tokens
        Assert.NotNull(resp);
        Assert.False(string.IsNullOrEmpty(resp.AccessToken));
        Assert.False(string.IsNullOrEmpty(resp.RefreshToken));

        // Login
        var loginResp = await authService.LoginAsync(new LoginRequest { Email = "testuser@example.com", Password = "P@ssw0rd" });
        Assert.NotNull(loginResp);
        Assert.False(string.IsNullOrEmpty(loginResp.AccessToken));
    }
}

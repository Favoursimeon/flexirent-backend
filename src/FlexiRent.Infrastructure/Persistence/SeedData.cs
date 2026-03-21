using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlexiRent.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db, IServiceProvider services)
    {
        await SeedAdminUserAsync(db, services);
    }

    private static async Task SeedAdminUserAsync(AppDbContext db, IServiceProvider services)
    {
        const string adminEmail = "admin@flexirent.com";

        if (await db.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        var adminPassword = services.GetRequiredService<IConfiguration>()
            .GetValue<string>("SeedData:AdminPassword")
            ?? throw new InvalidOperationException("SeedData:AdminPassword is not configured.");

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(adminUser);

        db.Profiles.Add(new Profile
        {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            FirstName = "FlexiRent",
            LastName = "Admin",
            CreatedAt = DateTime.UtcNow
        });

        db.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            Role = AppRole.Admin,
            AssignedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
}
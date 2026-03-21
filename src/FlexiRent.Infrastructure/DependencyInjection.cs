using FlexiRent.Infrastructure.Persistence;
using FlexiRent.Infrastructure.Services;
using FlexiRent.Infrastructure.Jobs;
using FlexiRent.Infrastructure.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Hangfire;
using Hangfire.PostgreSql;

namespace FlexiRent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Fix Npgsql DateTime UTC handling globally
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is not configured.")));

        // HTTP context (required for CurrentUserService)
        services.AddHttpContextAccessor();

        // Auth services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Profile
        services.AddScoped<IProfileService, ProfileService>();

        // Properties
        services.AddScoped<IPropertyService, PropertyService>();

        // Bookings
        services.AddScoped<IBookingService, BookingService>();

        // Payments
        services.AddHttpClient<IPaystackService, PaystackService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Notifications
        services.AddScoped<INotificationService, NotificationService>();

        // Admin
        services.AddScoped<IAdminService, AdminService>();

        // Jobs
        services.AddScoped<IPaymentSchedulerJob, PaymentSchedulerJob>();
        services.AddScoped<INotificationJob, NotificationJob>();

        // File storage
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Email
        services.AddScoped<IEmailService, SendGridEmailService>();

        // Cache
        services.AddScoped<ICacheService, CacheService>();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetValue<string>("Redis:ConnectionString")
                ?? throw new InvalidOperationException(
                    "Redis:ConnectionString is not configured.");
            options.InstanceName = "flexirent:";
        });

        // Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(
                    configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException(
                        "ConnectionStrings:DefaultConnection is not configured."))));

        services.AddHangfireServer();

        return services;
    }
}
using System.Text;
using AspNetCoreRateLimit;
using AutoMapper;
using FlexiRent.Api.Hubs;
using FlexiRent.Api.Middleware;
using FlexiRent.Application.Mappings;
using FlexiRent.Application.Validators;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Authorization;
using FlexiRent.Infrastructure.Jobs;
using FlexiRent.Infrastructure.Persistence;
using FlexiRent.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Npgsql DateTime UTC handling
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Load config
var configuration = builder.Configuration;

// Services
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

// Rate limiting
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

// Swagger
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo { Title = "FlexiRent API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter your JWT bearer token",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    opts.AddSecurityDefinition("Bearer", jwtScheme);
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins(
                configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? throw new InvalidOperationException("AllowedOrigins is not configured."))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// AutoMapper
var mapperConfig = new MapperConfiguration(mc => mc.AddProfile(new MappingProfile()));
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT Authentication
var jwtSettings = configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured."),
        ValidAudience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("JwtSettings:Audience is not configured."),
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs/chat") ||
                 path.StartsWithSegments("/hubs/notifications")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyConstants.RequireAdmin, policy =>
        policy.RequireRole(AppRole.Admin.ToString()));

    options.AddPolicy(PolicyConstants.RequireModerator, policy =>
        policy.RequireRole(AppRole.Moderator.ToString()));

    options.AddPolicy(PolicyConstants.RequireServiceProvider, policy =>
        policy.RequireRole(AppRole.ServiceProvider.ToString()));

    options.AddPolicy(PolicyConstants.RequireVendor, policy =>
        policy.RequireRole(AppRole.Vendor.ToString()));

    options.AddPolicy(PolicyConstants.RequireAdminOrModerator, policy =>
        policy.RequireRole(
            AppRole.Admin.ToString(),
            AppRole.Moderator.ToString()));

    options.AddPolicy(PolicyConstants.RequireVerifiedUser, policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("email_verified", "true"));
});

// Build app
var app = builder.Build();

// Handle reverse proxy headers
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseHttpsRedirection();

// Security headers
app.Use(async (context, next) =>
{
    // Skip restrictive headers for Swagger
    var isSwagger = context.Request.Path.StartsWithSegments("/swagger");

    if (!isSwagger)
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    }

    if (context.Request.IsHttps || !isSwagger)
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    context.Response.Headers.Append("Content-Security-Policy",
        isSwagger
            ? "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'"
            : "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self'");

    await next();
});

app.UseIpRateLimiting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Hangfire dashboard
if (app.Environment.IsDevelopment() || configuration.GetValue<bool>("Hangfire:Dashboard"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
    });
}

// Recurring jobs
RecurringJob.AddOrUpdate<IPaymentSchedulerJob>(
    "process-due-payments",
    job => job.ProcessDuePaymentsAsync(),
    Cron.Daily);

RecurringJob.AddOrUpdate<IPaymentSchedulerJob>(
    "mark-overdue-payments",
    job => job.MarkOverduePaymentsAsync(),
    Cron.Daily);

RecurringJob.AddOrUpdate<INotificationJob>(
    "send-payment-reminders",
    job => job.SendPaymentRemindersAsync(),
    Cron.Daily(8));

RecurringJob.AddOrUpdate<INotificationJob>(
    "send-property-match-alerts",
    job => job.SendPropertyMatchAlertsAsync(),
    Cron.Daily(9));

RecurringJob.AddOrUpdate<INotificationJob>(
    "send-lease-expiry-alerts",
    job => job.SendLeaseExpiryAlertsAsync(),
    Cron.Daily(8));

// Swagger
if (app.Environment.IsDevelopment() || configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlexiRent API v1");
        c.RoutePrefix = "swagger";
    }); app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlexiRent API v1");
        c.RoutePrefix = "swagger";
    });
}

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})).AllowAnonymous();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

// DB migrations and seeding
try
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(db, scope.ServiceProvider);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Database migration or seeding failed. Application cannot start.");
    throw;
}

app.Run();
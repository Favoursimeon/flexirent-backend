using System.Text;
using AutoMapper;
using FlexiRent.Api.Middleware;
using FlexiRent.Application.Mappings;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load config
var configuration = builder.Configuration;

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();
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
        Description = "Bearer token"
    };
    opts.AddSecurityDefinition("Bearer", jwtScheme);
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { jwtScheme, Array.Empty<string>() }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// EF Core + MySQL
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseMySql(configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))));

// AutoMapper
var mapperConfig = new MapperConfiguration(mc => { mc.AddProfile(new MappingProfile()); });
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// Infrastructure services
builder.Services.AddInfrastructureServices();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// Replace console email with SendGridEmailService (requires SendGrid:ApiKey in config)
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

// Authentication - JWT
var jwtSettings = configuration.GetSection("JwtSettings");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
    };
    // Allow SignalR to send access token via query string "access_token"
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs/chat")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Rate limiting - minimal example via middleware
builder.Services.AddMemoryCache();

// App
var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Swagger
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlexiRent API v1"));
}

app.MapControllers();
app.MapHub<FlexiRent.Api.Hubs.ChatHub>("/hubs/chat");

// Ensure DB created at startup for dev (use migrations in prod)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.InitializeAsync(db, scope.ServiceProvider);
}

app.Run();

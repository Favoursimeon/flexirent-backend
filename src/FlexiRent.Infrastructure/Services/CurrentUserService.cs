using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FlexiRent.Infrastructure.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
    bool IsEmailVerified { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(
            User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub),
            out var id) ? id : Guid.Empty;

    public string Email =>
        User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? string.Empty;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public bool IsEmailVerified =>
        User?.FindFirstValue("email_verified") == "true";

    public IEnumerable<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value)
        ?? Enumerable.Empty<string>();

    public bool IsInRole(string role) =>
        User?.IsInRole(role) ?? false;
}
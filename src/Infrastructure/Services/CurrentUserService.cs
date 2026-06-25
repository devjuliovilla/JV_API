using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Username { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public long? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim is not null && long.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User
        .FindAll(System.Security.Claims.ClaimTypes.Role)
        .Select(c => c.Value) ?? [];

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}

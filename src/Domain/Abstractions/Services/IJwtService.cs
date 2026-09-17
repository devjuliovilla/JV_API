using System.Security.Claims;

namespace Domain.Abstractions.Services;

public interface IJwtService
{
    (string token, DateTimeOffset expiresAt) GenerateAccessToken(Domain.Entities.User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}

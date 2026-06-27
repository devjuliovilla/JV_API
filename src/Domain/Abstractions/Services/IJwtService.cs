using System.Security.Claims;

namespace Domain.Abstractions.Services;

public interface IJwtService
{
    (string token, DateTime expiresAt) GenerateAccessToken(Domain.Entities.User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}

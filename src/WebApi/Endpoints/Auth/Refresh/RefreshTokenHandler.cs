using Microsoft.EntityFrameworkCore;
using MediatR;
using Infrastructure.Persistence;
using Domain.Abstractions.Services;
using Shared.DTOs.Auth;
using Shared.Exceptions;

namespace WebApi.Endpoints.Auth.Refresh;

public class RefreshTokenHandler(AppDbContext db, IJwtService jwtService) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            ?? throw new UnauthorizedException("Invalid refresh token");

        if (!storedToken.IsActive)
            throw new UnauthorizedException("Refresh token is expired or revoked");

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Name);
        var (newToken, expiresAt) = jwtService.GenerateAccessToken(user, roles);

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            ReplacedByToken = newToken
        };
        storedToken.ReplacedByToken = newRefreshToken.Token;

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = expiresAt
        };
    }
}

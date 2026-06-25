using Microsoft.EntityFrameworkCore;
using MediatR;
using JV_API.Infrastructure.Persistence;
using JV_API.Shared.Exceptions;

namespace JV_API.API.Endpoints.Auth.Revoke;

public class RevokeTokenHandler(AppDbContext db) : IRequestHandler<RevokeTokenCommand, Unit>
{
    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            ?? throw new NotFoundException("Refresh token not found");

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

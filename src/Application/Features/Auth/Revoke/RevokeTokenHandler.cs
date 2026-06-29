using Application.Abstractions.Persistence;
using Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Application.Features.Auth.Revoke;

public class RevokeTokenHandler(IAppDbContext db) : IRequestHandler<RevokeTokenCommand, Unit>
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

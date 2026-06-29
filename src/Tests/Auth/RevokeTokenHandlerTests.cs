using Application.Exceptions;
using Application.Features.Auth.Revoke;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tests.Auth;

public class RevokeTokenHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithExistingToken_RevokesToken()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var user = await context.Users.FirstAsync(u => u.Username == "testuser");
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await context.SaveChangesAsync();
        var handler = new RevokeTokenHandler(context);

        // Act
        await handler.Handle(new RevokeTokenCommand("refresh-token"), CancellationToken.None);

        // Assert
        var token = await context.RefreshTokens.FirstAsync(rt => rt.Token == "refresh-token");
        Assert.True(token.IsRevoked);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public async Task Handle_WithMissingToken_ThrowsNotFoundException()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new RevokeTokenHandler(context);

        // Act
        var act = () => handler.Handle(new RevokeTokenCommand("missing"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}

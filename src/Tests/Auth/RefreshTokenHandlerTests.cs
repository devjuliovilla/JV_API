using Application.Exceptions;
using Application.Features.Auth.Refresh;
using Domain.Entities;
using Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Tests.Auth;

public class RefreshTokenHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithActiveToken_RotatesRefreshToken()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var user = await context.Users.FirstAsync(u => u.Username == "testuser");
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "old-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        });
        await context.SaveChangesAsync();
        var jwtService = new FakeJwtService { AccessToken = "new-access-token", RefreshToken = "new-refresh-token" };
        var handler = new RefreshTokenHandler(context, jwtService, JwtOptions());

        // Act
        var response = await handler.Handle(new RefreshTokenCommand("old-refresh-token"), CancellationToken.None);

        // Assert
        var oldToken = await context.RefreshTokens.FirstAsync(rt => rt.Token == "old-refresh-token");
        var newToken = await context.RefreshTokens.FirstAsync(rt => rt.Token == "new-refresh-token");
        Assert.True(oldToken.IsRevoked);
        Assert.Equal("new-refresh-token", oldToken.ReplacedByToken);
        Assert.Equal(user.Id, newToken.UserId);
        Assert.Equal("new-access-token", response.Token);
        Assert.Equal("new-refresh-token", response.RefreshToken);
    }

    [Fact]
    public async Task Handle_WithMissingToken_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new RefreshTokenHandler(context, new FakeJwtService(), JwtOptions());

        // Act
        var act = () => handler.Handle(new RefreshTokenCommand("missing"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(act);
    }

    [Fact]
    public async Task Handle_WithRevokedToken_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var user = await context.Users.FirstAsync(u => u.Username == "testuser");
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = "revoked-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        });
        await context.SaveChangesAsync();
        var handler = new RefreshTokenHandler(context, new FakeJwtService(), JwtOptions());

        // Act
        var act = () => handler.Handle(new RefreshTokenCommand("revoked-refresh-token"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(act);
    }

    private static IOptions<JwtOptions> JwtOptions() => Options.Create(new JwtOptions());
}

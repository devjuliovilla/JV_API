using Application.Exceptions;
using Application.Features.Auth.Login;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Domain.Entities;

namespace Tests.Auth;

public class LoginHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var user = await context.Users.FirstAsync(u => u.Username == "testuser");
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Test123!");
        await context.SaveChangesAsync();
        var jwtService = new FakeJwtService { AccessToken = "access-token", RefreshToken = "refresh-token" };
        var handler = new LoginHandler(context, jwtService, NullLogger<LoginHandler>.Instance);

        // Act
        var response = await handler.Handle(new LoginCommand("testuser", "Test123!"), CancellationToken.None);

        // Assert
        Assert.Equal("access-token", response.Token);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.True(await context.RefreshTokens.AnyAsync(rt => rt.Token == "refresh-token"));
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var user = await context.Users.FirstAsync(u => u.Username == "testuser");
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Test123!");
        await context.SaveChangesAsync();
        var handler = new LoginHandler(context, new FakeJwtService(), NullLogger<LoginHandler>.Instance);

        // Act
        var act = () => handler.Handle(new LoginCommand("testuser", "WrongPassword"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(act);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var user = await context.Users.FirstAsync(u => u.Username == "testuser");
        user.IsActive = false;
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Test123!");
        await context.SaveChangesAsync();
        var handler = new LoginHandler(context, new FakeJwtService(), NullLogger<LoginHandler>.Instance);

        // Act
        var act = () => handler.Handle(new LoginCommand("testuser", "Test123!"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(act);
    }
}

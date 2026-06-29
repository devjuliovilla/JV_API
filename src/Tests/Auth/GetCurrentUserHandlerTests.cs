using Application.Exceptions;
using Application.Features.Auth.Me;
using Microsoft.EntityFrameworkCore;

namespace Tests.Auth;

public class GetCurrentUserHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithAuthenticatedUser_ReturnsCurrentUser()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var user = await context.Users.FirstAsync(u => u.Username == "testuser");
        var handler = new GetCurrentUserHandler(context, new FakeCurrentUserService(user.Id));

        // Act
        var response = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(user.Id, response.Id);
        Assert.Equal(user.Username, response.Username);
        Assert.Equal(user.Email, response.Email);
        Assert.Contains("Admin", response.Roles);
    }

    [Fact]
    public async Task Handle_WithoutAuthenticatedUser_ThrowsUnauthorizedException()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new GetCurrentUserHandler(context, new FakeCurrentUserService(null));

        // Act
        var act = () => handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UnauthorizedException>(act);
    }

    [Fact]
    public async Task Handle_WithMissingUser_ThrowsNotFoundException()
    {
        // Arrange
        using var context = CreateDbContext();
        var handler = new GetCurrentUserHandler(context, new FakeCurrentUserService(999));

        // Act
        var act = () => handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(act);
    }
}

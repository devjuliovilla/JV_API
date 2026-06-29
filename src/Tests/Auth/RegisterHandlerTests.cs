using Application.Exceptions;
using Application.Features.Auth.Register;
using Microsoft.EntityFrameworkCore;

namespace Tests.Auth;

public class RegisterHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesUserWithDefaultRole()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var handler = new RegisterHandler(context);
        var command = new RegisterCommand("newuser", "newuser@test.com", "Test123!");

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == response.UserId);
        Assert.NotNull(user);
        Assert.Equal(command.Username, response.Username);
        Assert.Equal(command.Email, user.Email);
        Assert.Contains(user.UserRoles, ur => ur.Role.Name == "User");
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    [Fact]
    public async Task Handle_WithDuplicateUsername_ThrowsConflictException()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var handler = new RegisterHandler(context);
        var command = new RegisterCommand("testuser", "unique@test.com", "Test123!");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ThrowsConflictException()
    {
        // Arrange
        using var context = CreateDbContext();
        await SeedData(context);
        var handler = new RegisterHandler(context);
        var command = new RegisterCommand("uniqueuser", "test@test.com", "Test123!");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task Handle_WithoutDefaultUserRole_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = CreateDbContext();
        context.Roles.Add(new Domain.Entities.Role { Name = "Admin" });
        await context.SaveChangesAsync();
        var handler = new RegisterHandler(context);
        var command = new RegisterCommand("newuser", "newuser@test.com", "Test123!");

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
    }
}

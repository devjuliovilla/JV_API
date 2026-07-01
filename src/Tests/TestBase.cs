using Microsoft.EntityFrameworkCore;
using Domain.Abstractions.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using System.Security.Claims;

namespace Tests;

public abstract class TestBase
{
    protected AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    protected async Task SeedData(AppDbContext context)
    {
        var adminRole = new Role { Name = "Admin" };
        var userRole = new Role { Name = "User" };
        context.Roles.AddRange(adminRole, userRole);

        var user = new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "AQAAAAIAAYagAAAAEOkXQJ3v8M5z5q0K3YxZ0g2b7c9d4f1h2j3k4l5m6n7o8p9q0r1s2t3u4v5w6x7y8z9=",
            IsActive = true
        };
        context.Users.Add(user);
        context.UserRoles.Add(new UserRole { User = user, Role = adminRole });

        await context.SaveChangesAsync();
    }

    protected sealed class FakeJwtService : IJwtService
    {
        public DateTime ExpiresAt { get; init; } = DateTime.UtcNow.AddMinutes(30);
        public string AccessToken { get; init; } = "access-token";
        public string RefreshToken { get; init; } = "refresh-token";

        public (string token, DateTime expiresAt) GenerateAccessToken(User user, IEnumerable<string> roles)
        {
            return (AccessToken, ExpiresAt);
        }

        public string GenerateRefreshToken()
        {
            return RefreshToken;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            return null;
        }
    }

    protected sealed class FakeCurrentUserService(long? userId) : ICurrentUserService
    {
        public long? UserId { get; } = userId;
        public string? Username => UserId.HasValue ? "testuser" : null;
        public IEnumerable<string> Roles => UserId.HasValue ? ["Admin"] : [];
        public bool IsAuthenticated => UserId.HasValue;
    }
}

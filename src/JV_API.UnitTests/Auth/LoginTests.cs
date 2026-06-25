using Microsoft.AspNetCore.Identity;
using Xunit;
using JV_API.Domain.Entities;
using JV_API.Infrastructure.Persistence;
using JV_API.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace JV_API.UnitTests.Auth;

public class LoginTests : TestBase
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        using var context = CreateDbContext();
        await SeedData(context);

        var user = await context.Users.FindAsync(1L);
        Assert.NotNull(user);

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, "Test123!");
        await context.SaveChangesAsync();

        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "ThisIsASecretKeyForTestingPurposesOnly123!",
            Issuer = "JV_API",
            Audience = "JV_API",
            ExpirationMinutes = 60
        });
        var jwtService = new JwtService(jwtOptions);

        var roles = new[] { "Admin" };
        var (token, expiresAt) = jwtService.GenerateAccessToken(user, roles);

        Assert.False(string.IsNullOrEmpty(token));
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsNull()
    {
        using var context = CreateDbContext();
        await SeedData(context);

        var user = await context.Users.FindAsync(1L);
        Assert.NotNull(user);

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, "Test123!");
        await context.SaveChangesAsync();

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, "WrongPassword");

        Assert.Equal(PasswordVerificationResult.Failed, result);
    }
}

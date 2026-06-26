using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Shared.DTOs.Auth;
using Shared.Exceptions;

namespace WebApi.Endpoints.Auth.Login;

public class LoginHandler(AppDbContext db, IJwtService jwtService, ILogger<LoginHandler> logger) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedException("User is inactive");

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Invalid credentials");

        var roles = user.UserRoles.Select(ur => ur.Role.Name);
        var (token, expiresAt) = jwtService.GenerateAccessToken(user, roles);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {Username} (id: {UserId}) logged in successfully.", user.Username, user.Id);

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt
        };
    }
}

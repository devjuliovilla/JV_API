using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Domain.Entities;
using Infrastructure.Persistence;
using Shared.DTOs.Auth;
using Shared.Exceptions;

namespace WebApi.Endpoints.Auth.Register;

public class RegisterHandler(AppDbContext db) : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
            throw new Shared.Exceptions.ValidationException("Username already exists");

        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new Shared.Exceptions.ValidationException("Email already exists");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            IsActive = true
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        var defaultRole = await db.Roles.FirstAsync(r => r.Name == "User", cancellationToken);
        db.UserRoles.Add(new UserRole { User = user, Role = defaultRole });

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return new RegisterResponse
        {
            UserId = user.Id,
            Username = user.Username
        };
    }
}

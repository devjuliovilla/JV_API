using Application.Abstractions.Persistence;
using Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Domain.Entities;
using Application.Features.Auth.Register;

namespace Application.Features.Auth.Register;

public class RegisterHandler(IAppDbContext db) : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
            throw new ConflictException("Username already exists");

        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new ConflictException("Email already exists");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            IsActive = true
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken)
            ?? throw new InvalidOperationException("Default role 'User' is not configured.");
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

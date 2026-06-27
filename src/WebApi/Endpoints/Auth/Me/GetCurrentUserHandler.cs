using Microsoft.EntityFrameworkCore;
using MediatR;
using Infrastructure.Persistence;
using Domain.Abstractions.Services;
using Shared.DTOs.Auth;
using Shared.Exceptions;

namespace WebApi.Endpoints.Auth.Me;

public class GetCurrentUserHandler(AppDbContext db, ICurrentUserService currentUser) : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Not authenticated");

        var user = await db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User not found");

        return new CurrentUserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }
}

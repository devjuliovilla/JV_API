using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin", cancellationToken);
        if (adminRole is null)
        {
            adminRole = new Role { Name = "Admin", Description = "Administrator" };
            context.Roles.Add(adminRole);
        }

        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User", cancellationToken);
        if (userRole is null)
        {
            userRole = new Role { Name = "User", Description = "Standard user" };
            context.Roles.Add(userRole);
        }

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin", cancellationToken);
        if (adminUser is null)
        {
            adminUser = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                IsActive = true
            };
            adminUser.PasswordHash = new PasswordHasher<User>().HashPassword(adminUser, "Admin123!");
            context.Users.Add(adminUser);
        }

        var hasAdminRole = await context.UserRoles
            .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id, cancellationToken);
        if (!hasAdminRole)
        {
            context.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

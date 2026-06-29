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

        var electronics = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics", cancellationToken);
        if (electronics is null)
        {
            electronics = new Category { Name = "Electronics", Description = "Electronic items" };
            context.Categories.Add(electronics);
        }

        var books = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Books", cancellationToken);
        if (books is null)
        {
            books = new Category { Name = "Books", Description = "Books and publications" };
            context.Categories.Add(books);
        }

        if (!await context.Products.AnyAsync(p => p.Name == "Laptop", cancellationToken))
        {
            context.Products.Add(new Product { Name = "Laptop", Description = "High performance laptop", Price = 1200.00m, Category = electronics });
        }

        if (!await context.Products.AnyAsync(p => p.Name == "Mouse", cancellationToken))
        {
            context.Products.Add(new Product { Name = "Mouse", Description = "Wireless mouse", Price = 25.00m, Category = electronics });
        }

        if (!await context.Products.AnyAsync(p => p.Name == ".NET Guide", cancellationToken))
        {
            context.Products.Add(new Product { Name = ".NET Guide", Description = "Complete .NET guide", Price = 45.00m, Category = books });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

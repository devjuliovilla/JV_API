using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var adminRole = new Role { Name = "Admin", Description = "Administrator" };
        var userRole = new Role { Name = "User", Description = "Standard user" };
        context.Roles.AddRange(adminRole, userRole);

        var hasher = new PasswordHasher<User>();
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@example.com",
            IsActive = true
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");
        context.Users.Add(adminUser);

        context.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });

        var category1 = new Category { Name = "Electronics", Description = "Electronic items" };
        var category2 = new Category { Name = "Books", Description = "Books and publications" };
        context.Categories.AddRange(category1, category2);

        context.Products.AddRange(
            new Product { Name = "Laptop", Description = "High performance laptop", Price = 1200.00m, Category = category1 },
            new Product { Name = "Mouse", Description = "Wireless mouse", Price = 25.00m, Category = category1 },
            new Product { Name = ".NET Guide", Description = "Complete .NET guide", Price = 45.00m, Category = category2 }
        );

        await context.SaveChangesAsync();
    }
}

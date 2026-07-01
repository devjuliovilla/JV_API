using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Seed;

public static class SeedData
{
    private static readonly (string Name, string Description)[] CategorySeeds =
    [
        ("Electronics", "Electronic items"),
        ("Books", "Books and publications"),
        ("Home", "Home and kitchen products"),
        ("Sports", "Sports and fitness gear"),
        ("Office", "Office essentials and accessories")
    ];

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

        var categoriesByName = await EnsureCategoriesAsync(context, cancellationToken);
        await EnsureProductsAsync(context, categoriesByName, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Dictionary<string, Category>> EnsureCategoriesAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var existingCategories = await context.Categories
            .ToDictionaryAsync(category => category.Name, cancellationToken);

        foreach (var (name, description) in CategorySeeds)
        {
            if (existingCategories.ContainsKey(name))
            {
                continue;
            }

            var category = new Category
            {
                Name = name,
                Description = description
            };

            context.Categories.Add(category);
            existingCategories[name] = category;
        }

        return existingCategories;
    }

    private static async Task EnsureProductsAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, Category> categoriesByName,
        CancellationToken cancellationToken)
    {
        var existingProductNames = await context.Products
            .Select(product => product.Name)
            .ToHashSetAsync(cancellationToken);

        foreach (var (categoryName, _) in CategorySeeds)
        {
            var category = categoriesByName[categoryName];

            for (var index = 1; index <= 40; index++)
            {
                var productName = $"{categoryName} Product {index:000}";
                if (existingProductNames.Contains(productName))
                {
                    continue;
                }

                context.Products.Add(new Product
                {
                    Name = productName,
                    Description = $"Sample {categoryName.ToLowerInvariant()} product #{index}.",
                    Price = 10m + (index * 3.5m),
                    Category = category
                });

                existingProductNames.Add(productName);
            }
        }
    }
}

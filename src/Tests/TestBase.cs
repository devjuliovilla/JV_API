using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Persistence;

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
        var role = new Role { Name = "Admin" };
        context.Roles.Add(role);

        var user = new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "AQAAAAIAAYagAAAAEOkXQJ3v8M5z5q0K3YxZ0g2b7c9d4f1h2j3k4l5m6n7o8p9q0r1s2t3u4v5w6x7y8z9=",
            IsActive = true
        };
        context.Users.Add(user);
        context.UserRoles.Add(new UserRole { User = user, Role = role });

        var category = new Category { Name = "Test Category" };
        context.Categories.Add(category);

        context.Products.AddRange(
            new Product { Name = "Product 1", Price = 10.00m, Category = category },
            new Product { Name = "Product 2", Price = 20.00m, Category = category }
        );

        await context.SaveChangesAsync();
    }
}

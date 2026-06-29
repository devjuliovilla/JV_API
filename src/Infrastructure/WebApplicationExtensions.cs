using Infrastructure.Persistence;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (app.Configuration.GetValue("Database:ApplyMigrations", true))
        {
            await context.Database.MigrateAsync();
        }

        if (app.Configuration.GetValue("Database:SeedOnStartup", true))
        {
            await SeedData.InitializeAsync(context);
        }
    }
}

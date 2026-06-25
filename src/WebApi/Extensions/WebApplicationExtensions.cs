using Microsoft.EntityFrameworkCore;
using Serilog;
using Infrastructure.Persistence;
using Infrastructure.Seed;
using WebApi.Middleware;

namespace WebApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRateLimiter();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (app.Environment.IsDevelopment())
        {
            await context.Database.MigrateAsync();
            await SeedData.InitializeAsync(context);
        }
    }
}

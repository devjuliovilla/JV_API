using Hangfire;
using Microsoft.EntityFrameworkCore;
using Serilog;
using JV_API.Infrastructure.Persistence;
using JV_API.Infrastructure.Seed;
using JV_API.API.Middleware;

namespace JV_API.API.Extensions;

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

        var hangfireEnabled = app.Configuration.GetSection("Hangfire").GetValue<bool>("Enabled", false);
        if (hangfireEnabled)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = []
            });
        }

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

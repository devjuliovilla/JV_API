using WebApi.Extensions;
using WebApi.Endpoints;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.MapGroup("/api/v1/auth").MapAuthEndpoints();
app.MapGroup("/api/v1/products").MapProductEndpoints();

app.MapGet("/health", async (HealthCheckService healthCheck, AppDbContext db) =>
{
    Log.Information("Health check requested");

    var report = await healthCheck.CheckHealthAsync();

    return Results.Ok(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description
        })
    });
});

await app.InitializeDatabaseAsync();

app.Run();

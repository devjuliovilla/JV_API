using WebApi.Extensions;
using WebApi.Endpoints;
using Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);

    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        config.WriteTo.MSSqlServer(
            connectionString: connectionString,
            sinkOptions: new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                SchemaName = "audit",
                AutoCreateSqlTable = true
            });
    }
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.MapGroup("/api/v1/auth").MapAuthEndpoints();
app.MapGroup("/api/v1/logs").MapLogEndpoints();
app.MapGroup("/api/v1/products").MapProductEndpoints();

app.MapGet("/health", async (HealthCheckService healthCheck, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    var report = await healthCheck.CheckHealthAsync(cancellationToken);
    var checkStatuses = report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString());

    logger.LogInformation("Health check requested. Status: {Status}", report.Status);

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

using JV_API.API.Extensions;
using JV_API.API.Endpoints;
using JV_API.Infrastructure;
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
app.MapHealthChecks("/health");

await app.InitializeDatabaseAsync();

app.Run();

using WebApi.Extensions;
using WebApi.Endpoints;
using Infrastructure;
using Serilog;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilog();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddEndpoints();

var app = builder.Build();

app.ConfigurePipeline();

app.MapEndpoints();
app.MapHealthEndpoint();

await app.InitializeDatabaseAsync();

app.Run();

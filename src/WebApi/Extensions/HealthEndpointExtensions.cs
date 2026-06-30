using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebApi.Constants;

namespace WebApi.Extensions;

public static class HealthEndpointExtensions
{
    public static WebApplication MapHealthEndpoint(this WebApplication app)
    {
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
        })
            .WithTags(EndpointTags.Health)
            .WithName(EndpointNames.Health)
        .WithDescription(EndpointDescriptions.Health);

        return app;
    }
}
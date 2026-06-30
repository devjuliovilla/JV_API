using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointInterface = typeof(IEndpoint);

        var endpointTypes = endpointInterface.Assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                endpointInterface.IsAssignableFrom(type));

        var groups = new Dictionary<string, RouteGroupBuilder>();

        foreach (var endpointType in endpointTypes)
        {
            var endpoint = (IEndpoint)app.Services.GetRequiredService(endpointType);

            if (!groups.TryGetValue(endpoint.Group, out var group))
            {
                group = app.MapGroup(endpoint.Group)
                    .WithTags(endpoint.Tag);

                groups.Add(endpoint.Group, group);
            }

            endpoint.Map(group);
        }

        return app;
    }
}
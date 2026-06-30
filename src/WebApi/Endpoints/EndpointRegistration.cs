using Microsoft.Extensions.DependencyInjection;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints;

public static class EndpointRegistration
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        var endpointType = typeof(IEndpoint);

        var endpointTypes = endpointType.Assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                endpointType.IsAssignableFrom(type));

        foreach (var endpointTypeToRegister in endpointTypes)
        {
            services.AddTransient(endpointTypeToRegister);
        }

        return services;
    }
}
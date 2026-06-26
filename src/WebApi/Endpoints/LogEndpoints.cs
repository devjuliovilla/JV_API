using WebApi.Endpoints.Logs.Test;

namespace WebApi.Endpoints;

public static class LogEndpoints
{
    public static void MapLogEndpoints(this IEndpointRouteBuilder group)
    {
        TestLogEndpoint.Map(group);
    }
}

using WebApi.Constants;

namespace WebApi.Endpoints.Abstractions;

public abstract class LogsEndpoint : IEndpoint
{
    public string Group => ApiGroups.Logs;
    public string Tag => EndpointTags.Logs;

    public abstract void Map(RouteGroupBuilder group);
}
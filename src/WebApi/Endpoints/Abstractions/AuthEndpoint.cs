using WebApi.Constants;

namespace WebApi.Endpoints.Abstractions;

public abstract class AuthEndpoint : IEndpoint
{
    public string Group => ApiGroups.Auth;
    public string Tag => EndpointTags.Auth;

    public abstract void Map(RouteGroupBuilder group);
}
using WebApi.Endpoints.Auth.Login;
using WebApi.Endpoints.Auth.Register;
using WebApi.Endpoints.Auth.Refresh;
using WebApi.Endpoints.Auth.Revoke;
using WebApi.Endpoints.Auth.Me;

namespace WebApi.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder group)
    {
        LoginEndpoint.Map(group);
        RegisterEndpoint.Map(group);
        RefreshTokenEndpoint.Map(group);
        RevokeTokenEndpoint.Map(group);
        GetCurrentUserEndpoint.Map(group);
    }
}

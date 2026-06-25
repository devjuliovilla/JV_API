using JV_API.API.Endpoints.Auth.Login;
using JV_API.API.Endpoints.Auth.Register;
using JV_API.API.Endpoints.Auth.Refresh;
using JV_API.API.Endpoints.Auth.Revoke;
using JV_API.API.Endpoints.Auth.Me;

namespace JV_API.API.Endpoints;

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

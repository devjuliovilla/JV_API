using Application.Features.Auth.Me;
using MediatR;

namespace WebApi.Endpoints.Auth.Me;

public static class GetCurrentUserEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/me", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery());
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithDescription("Get current authenticated user info");
    }
}

using Application.Features.Auth.Me;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Auth;

public sealed class GetCurrentUserEndpoint : AuthEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet(AuthRoutes.Me, async (ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName(EndpointNames.GetCurrentUser)
        .WithDescription(EndpointDescriptions.GetCurrentUser);
    }
}

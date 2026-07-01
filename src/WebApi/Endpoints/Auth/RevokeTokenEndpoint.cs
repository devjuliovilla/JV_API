using Application.Features.Auth.Revoke;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Auth;

public sealed class RevokeTokenEndpoint : AuthEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoutes.Revoke, async (RevokeTokenRequest request, ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RevokeTokenCommand(request.RefreshToken);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName(EndpointNames.RevokeToken)
        .WithDescription(EndpointDescriptions.RevokeToken);
    }
}

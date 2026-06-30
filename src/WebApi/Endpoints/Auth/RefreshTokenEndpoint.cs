using Application.Features.Auth.Refresh;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Auth;

public sealed class RefreshTokenEndpoint : AuthEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoutes.Refresh, async (RefreshTokenRequest request, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName(EndpointNames.RefreshToken)
.WithDescription(EndpointDescriptions.RefreshToken);
    }
}

using Application.Features.Auth.Login;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Auth;

public sealed class LoginEndpoint : AuthEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoutes.Login, async (LoginRequest request, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginCommand(request.Username, request.Password);
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName(EndpointNames.Login)
        .WithDescription(EndpointDescriptions.Login);
    }
}
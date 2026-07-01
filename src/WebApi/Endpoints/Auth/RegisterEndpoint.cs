using Application.Features.Auth.Register;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Auth;

public sealed class RegisterEndpoint : AuthEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(AuthRoutes.Register, async (RegisterRequest request, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCommand(request.Username, request.Email, request.Password);
            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/auth/me", result);
        })
        .AllowAnonymous()
        .WithName(EndpointNames.Register)
        .WithDescription(EndpointDescriptions.Register);
    }
}

using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Revoke;

public static class RevokeTokenEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/revoke", async (RevokeTokenRequest request, ISender sender) =>
        {
            var command = new RevokeTokenCommand(request.RefreshToken);
            await sender.Send(command);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("RevokeToken")
        .WithDescription("Revoke a refresh token");
    }
}

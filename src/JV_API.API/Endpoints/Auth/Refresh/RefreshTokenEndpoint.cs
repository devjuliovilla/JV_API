using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Refresh;

public static class RefreshTokenEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/refresh", async (RefreshTokenRequest request, ISender sender) =>
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("RefreshToken")
        .WithDescription("Refresh JWT token using refresh token");
    }
}

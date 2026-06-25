using MediatR;
using JV_API.Shared.DTOs.Auth;

namespace JV_API.API.Endpoints.Auth.Register;

public static class RegisterEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/register", async (RegisterRequest request, ISender sender) =>
        {
            var command = new RegisterCommand(request.Username, request.Email, request.Password);
            var result = await sender.Send(command);
            return Results.Created($"/api/v1/auth/me", result);
        })
        .AllowAnonymous()
        .WithName("Register")
        .WithDescription("Register a new user");
    }
}

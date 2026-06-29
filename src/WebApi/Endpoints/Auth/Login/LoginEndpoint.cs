using Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebApi.Endpoints.Auth.Login;

public static class LoginEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/login", async (LoginRequest request, ISender sender) =>
        {
            var command = new LoginCommand(request.Username, request.Password);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithDescription("Authenticate user and return JWT token");
    }
}

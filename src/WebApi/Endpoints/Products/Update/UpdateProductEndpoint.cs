using Application.Features.Products.Update;
using MediatR;

namespace WebApi.Endpoints.Products.Update;

public static class UpdateProductEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPut("/{id:long}", async (long id, UpdateProductRequest request, ISender sender) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.CategoryId);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("UpdateProduct")
        .WithDescription("Update an existing product");
    }
}

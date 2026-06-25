using MediatR;
using Shared.DTOs.Products;

namespace WebApi.Endpoints.Products.Create;

public static class CreateProductEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapPost("/", async (CreateProductRequest request, ISender sender) =>
        {
            var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.CategoryId);
            var result = await sender.Send(command);
            return Results.Created($"/api/v1/products/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithName("CreateProduct")
        .WithDescription("Create a new product");
    }
}

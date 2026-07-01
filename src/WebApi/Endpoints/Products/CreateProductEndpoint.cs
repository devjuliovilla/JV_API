using Application.Features.Products.Create;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Products;

public sealed class CreateProductEndpoint : ProductsEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPost(ProductRoutes.Root, async (CreateProductRequest request, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateProductCommand(request.Name, request.Description, request.Price, request.CategoryId);
            var result = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/products/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithName(EndpointNames.CreateProduct)
        .WithDescription(EndpointDescriptions.CreateProduct);
    }
}

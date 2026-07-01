using Application.Features.Products.Update;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Products;

public sealed class UpdateProductEndpoint : ProductsEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapPut(ProductRoutes.ById, async (long id, UpdateProductRequest request, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.CategoryId);
            var result = await sender.Send(command, cancellationToken);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName(EndpointNames.UpdateProduct)
        .WithDescription(EndpointDescriptions.UpdateProduct);
    }
}

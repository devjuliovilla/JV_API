using Application.Features.Products.Delete;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Products;

public sealed class DeleteProductEndpoint : ProductsEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapDelete(ProductRoutes.ById, async (long id, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            await sender.Send(new DeleteProductCommand(id), cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName(EndpointNames.DeleteProduct)
.WithDescription(EndpointDescriptions.DeleteProduct);
    }
}

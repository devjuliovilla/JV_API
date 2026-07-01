using Application.Features.Products.Common;
using Application.Features.Products.Get;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Products;

public sealed class GetProductEndpoint : ProductsEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet(ProductRoutes.ById, async (long id, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetProductQuery(id), cancellationToken);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName(EndpointNames.GetProduct)
        .WithDescription(EndpointDescriptions.GetProduct);
    }
}

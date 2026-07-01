using Application.Features.Products.Common;
using Application.Features.Products.List;
using MediatR;
using WebApi.Constants;
using WebApi.Endpoints.Abstractions;

namespace WebApi.Endpoints.Products;

public sealed class GetProductsEndpoint : ProductsEndpoint
{
    public override void Map(RouteGroupBuilder group)
    {
        group.MapGet(ProductRoutes.Root, async ([AsParameters] GetProductsQuery query, 
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName(EndpointNames.GetProducts)
        .WithDescription(EndpointDescriptions.GetProducts);
    }
}

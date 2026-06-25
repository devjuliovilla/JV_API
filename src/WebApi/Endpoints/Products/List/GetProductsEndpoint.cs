using MediatR;
using Shared.DTOs;
using Shared.DTOs.Products;

namespace WebApi.Endpoints.Products.List;

public static class GetProductsEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/", async ([AsParameters] GetProductsQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("GetProducts")
        .WithDescription("Get paginated list of products");
    }
}

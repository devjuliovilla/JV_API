using MediatR;
using JV_API.Shared.DTOs.Products;

namespace JV_API.API.Endpoints.Products.Get;

public static class GetProductEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapGet("/{id:long}", async (long id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductQuery(id));
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("GetProduct")
        .WithDescription("Get a product by id");
    }
}

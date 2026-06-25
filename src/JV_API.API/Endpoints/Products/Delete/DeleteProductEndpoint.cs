using MediatR;

namespace JV_API.API.Endpoints.Products.Delete;

public static class DeleteProductEndpoint
{
    public static void Map(IEndpointRouteBuilder group)
    {
        group.MapDelete("/{id:long}", async (long id, ISender sender) =>
        {
            await sender.Send(new DeleteProductCommand(id));
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("DeleteProduct")
        .WithDescription("Delete a product (soft delete)");
    }
}

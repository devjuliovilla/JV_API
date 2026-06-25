using WebApi.Endpoints.Products.Create;
using WebApi.Endpoints.Products.Update;
using WebApi.Endpoints.Products.Delete;
using WebApi.Endpoints.Products.Get;
using WebApi.Endpoints.Products.List;

namespace WebApi.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder group)
    {
        CreateProductEndpoint.Map(group);
        UpdateProductEndpoint.Map(group);
        DeleteProductEndpoint.Map(group);
        GetProductEndpoint.Map(group);
        GetProductsEndpoint.Map(group);
    }
}

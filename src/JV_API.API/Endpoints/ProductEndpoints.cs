using JV_API.API.Endpoints.Products.Create;
using JV_API.API.Endpoints.Products.Update;
using JV_API.API.Endpoints.Products.Delete;
using JV_API.API.Endpoints.Products.Get;
using JV_API.API.Endpoints.Products.List;

namespace JV_API.API.Endpoints;

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

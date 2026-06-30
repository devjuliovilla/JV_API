using WebApi.Constants;

namespace WebApi.Endpoints.Abstractions;

public abstract class ProductsEndpoint : IEndpoint
{
    public string Group => ApiGroups.Products;
    public string Tag => EndpointTags.Products;

    public abstract void Map(RouteGroupBuilder group);
}
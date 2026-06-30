namespace WebApi.Endpoints.Abstractions;

public interface IEndpoint
{
    string Group { get; }
    string Tag { get; }
    void Map(RouteGroupBuilder group);
}
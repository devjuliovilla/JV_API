namespace WebApi.Constants;

public static class ApiGroups
{
    public const string Auth = $"{ApiRoutes.Api}{ApiVersions.Current}/auth";
    public const string Logs = $"{ApiRoutes.Api}{ApiVersions.Current}/logs";
    public const string Products = $"{ApiRoutes.Api}{ApiVersions.Current}/products";
}

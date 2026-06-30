namespace WebApi.Constants;

public static class EndpointDescriptions
{
    // Auth
    #region Auth

    public const string Login = "Authenticate user and return JWT token.";
    public const string Register = "Register a new user.";
    public const string RefreshToken = "Refresh the access token.";
    public const string RevokeToken = "Revoke a refresh token.";
    public const string GetCurrentUser = "Get the current authenticated user.";

    #endregion

    // Products
    #region Products

    public const string GetProducts = "Retrieve all products.";
    public const string GetProduct = "Retrieve a product by id.";
    public const string CreateProduct = "Create a new product.";
    public const string UpdateProduct = "Update an existing product.";
    public const string DeleteProduct = "Delete a product.";

    #endregion

    // Logs
    #region Logs

    public const string GetLogs = "Retrieve application logs.";

    #endregion

    // Health
    #region Health

    public const string Health = "Check application health.";

    #endregion
}
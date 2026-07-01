namespace WebApi.Constants;

public static class EndpointNames
{
    // Auth
    #region Auth

    public const string Login = nameof(Login);
    public const string Register = nameof(Register);
    public const string RefreshToken = nameof(RefreshToken);
    public const string RevokeToken = nameof(RevokeToken);
    public const string GetCurrentUser = nameof(GetCurrentUser);

    #endregion

    // Products
    #region Products

    public const string GetProducts = nameof(GetProducts);
    public const string GetProduct = nameof(GetProduct);
    public const string CreateProduct = nameof(CreateProduct);
    public const string UpdateProduct = nameof(UpdateProduct);
    public const string DeleteProduct = nameof(DeleteProduct);

    #endregion

    // Logs
    #region Logs

    public const string TestLog = nameof(TestLog);

    #endregion

    // Health
    #region Health

    public const string Health = nameof(Health);

    #endregion
}

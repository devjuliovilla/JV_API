namespace WebApi.Constants;

public static class EndpointDescriptions
{
    public const string Login = "Authenticate user and return JWT token.";
    public const string Register = "Register a new user.";
    public const string RefreshToken = "Refresh the access token.";
    public const string RevokeToken = "Revoke a refresh token.";
    public const string GetCurrentUser = "Get the current authenticated user.";

    public const string Health = "Check application health.";
}

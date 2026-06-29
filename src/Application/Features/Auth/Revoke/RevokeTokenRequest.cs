namespace Application.Features.Auth.Revoke;

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

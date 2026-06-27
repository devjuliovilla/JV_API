namespace Domain.Settings;

public class JwtOptions
{
    public const string Section = "Jwt";
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "WebApi";
    public string Audience { get; set; } = "WebApi";
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 1;
}

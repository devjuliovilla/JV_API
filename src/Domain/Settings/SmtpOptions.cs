namespace Domain.Settings;

public class SmtpOptions
{
    public const string Section = "Smtp";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = "Web API";
    public bool EnableSsl { get; set; } = true;
}

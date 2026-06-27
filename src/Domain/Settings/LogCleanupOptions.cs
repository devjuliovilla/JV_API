namespace Domain.Settings;

public class LogCleanupOptions
{
    public const string Section = "LogCleanup";
    public int RetentionDays { get; set; } = 7;
    public int RunIntervalHours { get; set; } = 24;
}

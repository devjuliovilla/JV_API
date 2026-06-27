namespace Domain.Settings;

public class FileStorageOptions
{
    public const string Section = "FileStorage";
    public string LocalPath { get; set; } = "App_Data/Storage";
}

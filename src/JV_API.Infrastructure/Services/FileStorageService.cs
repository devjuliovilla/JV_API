using Microsoft.Extensions.Options;

namespace JV_API.Infrastructure.Services;

public class FileStorageOptions
{
    public const string Section = "FileStorage";
    public string LocalPath { get; set; } = "App_Data/Storage";
}

public interface IFileStorageService
{
    Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream?> GetAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}

public class LocalFileStorageService(IOptions<FileStorageOptions> options) : IFileStorageService
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<string> SaveAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.LocalPath);
        Directory.CreateDirectory(directory);

        var uniqueName = $"{Guid.NewGuid():N}_{fileName}";
        var filePath = Path.Combine(directory, uniqueName);

        await using var stream = File.Create(filePath);
        await content.CopyToAsync(stream, cancellationToken);

        return Path.Combine(_options.LocalPath, uniqueName);
    }

    public Task<Stream?> GetAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}

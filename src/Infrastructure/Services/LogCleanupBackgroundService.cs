using Domain.Abstractions.Services;
using Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class LogCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<LogCleanupOptions> options,
    ILogger<LogCleanupBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly LogCleanupOptions _options = options.Value;
    private readonly ILogger<LogCleanupBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunCleanupAsync(stoppingToken);

        var intervalHours = Math.Max(1, _options.RunIntervalHours);
        using var timer = new PeriodicTimer(TimeSpan.FromHours(intervalHours));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCleanupAsync(stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cleanupService = scope.ServiceProvider.GetRequiredService<ILogCleanupService>();
            var deletedCount = await cleanupService.DeleteExpiredAsync(DateTime.UtcNow, cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("Deleted {DeletedCount} expired log entries.", deletedCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while cleaning expired logs.");
        }
    }
}

using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Tests.Infrastructure;

public class LogCleanupServiceTests : TestBase
{
    [Fact]
    public async Task DeleteExpiredAsync_RemovesOnlyLogsOlderThanRetention()
    {
        await using var context = CreateDbContext();
        var options = Options.Create(new LogCleanupOptions { RetentionDays = 7, RunIntervalHours = 24 });
        var now = new DateTime(2026, 6, 25, 12, 0, 0, DateTimeKind.Utc);

        context.Logs.AddRange(
            new LogEntry { Level = "Error", Message = "Old log", CreatedAt = now.AddDays(-8), CreatedBy = "system" },
            new LogEntry { Level = "Info", Message = "Recent log", CreatedAt = now.AddDays(-2), CreatedBy = "system" });

        await context.SaveChangesAsync();

        var service = new LogCleanupService(context, options);

        var deletedCount = await service.DeleteExpiredAsync(now);

        Assert.Equal(1, deletedCount);
        Assert.Single(context.Logs);
        Assert.Equal("Recent log", context.Logs.Single().Message);
    }
}

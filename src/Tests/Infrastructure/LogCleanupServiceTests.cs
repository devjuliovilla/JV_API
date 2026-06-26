using Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Tests.Infrastructure;

public class LogCleanupServiceTests
{
    [Fact]
    public async Task DeleteExpiredAsync_WithZeroRetention_DeletesAll()
    {
        var options = Options.Create(new LogCleanupOptions { RetentionDays = 0 });
        var now = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        DateTime? capturedCutoff = null;

        var service = new TestableLogCleanupService(options, cutoff => capturedCutoff = cutoff);

        var result = await service.DeleteExpiredAsync(now);

        Assert.Equal(5, result);
        Assert.Equal(now, capturedCutoff);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WithRetentionDays_CalculatesCorrectCutoff()
    {
        var options = Options.Create(new LogCleanupOptions { RetentionDays = 7 });
        var now = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        DateTime? capturedCutoff = null;

        var service = new TestableLogCleanupService(options, cutoff => capturedCutoff = cutoff);

        await service.DeleteExpiredAsync(now);

        Assert.Equal(now.AddDays(-7), capturedCutoff);
    }

    [Fact]
    public async Task DeleteExpiredAsync_WithNegativeRetention_DefaultsToZero()
    {
        var options = Options.Create(new LogCleanupOptions { RetentionDays = -5 });
        var now = new DateTime(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
        DateTime? capturedCutoff = null;

        var service = new TestableLogCleanupService(options, cutoff => capturedCutoff = cutoff);

        await service.DeleteExpiredAsync(now);

        Assert.Equal(now, capturedCutoff);
    }

    private class TestableLogCleanupService : LogCleanupService
    {
        private readonly Action<DateTime> _onExecute;

        public TestableLogCleanupService(IOptions<LogCleanupOptions> options, Action<DateTime> onExecute)
            : base(null!, options)
        {
            _onExecute = onExecute;
        }

        protected override Task<int> ExecuteDeleteAsync(DateTime cutoff, CancellationToken cancellationToken = default)
        {
            _onExecute(cutoff);
            return Task.FromResult(5);
        }
    }
}

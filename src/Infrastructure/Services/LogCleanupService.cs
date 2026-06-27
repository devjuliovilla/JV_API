using Domain.Abstractions.Services;
using Domain.Settings;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class LogCleanupService(AppDbContext dbContext, IOptions<LogCleanupOptions> options) : ILogCleanupService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly LogCleanupOptions _options = options.Value;

    public async Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var retentionDays = Math.Max(0, _options.RetentionDays);
        var cutoff = utcNow.AddDays(-retentionDays);
        return await ExecuteDeleteAsync(cutoff, cancellationToken);
    }

    protected virtual async Task<int> ExecuteDeleteAsync(DateTime cutoff, CancellationToken cancellationToken)
    {
        try
        {
            var deletedCount = await _dbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM [audit].[Logs] WHERE [TimeStamp] < {0}", [cutoff], cancellationToken);

            return deletedCount;
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 208)
        {
            return 0;
        }
    }
}

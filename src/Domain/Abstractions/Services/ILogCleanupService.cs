namespace Domain.Abstractions.Services;

public interface ILogCleanupService
{
    Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
}

namespace Tests.Infrastructure;

public class LogCleanupServiceTests : TestBase
{
    // LogCleanupService now uses raw SQL against [audit].[Logs] via ExecuteSqlRawAsync.
    // Unit tests require a real SQL Server instance. Integration tests can be added here
    // pointing to a test database.
}

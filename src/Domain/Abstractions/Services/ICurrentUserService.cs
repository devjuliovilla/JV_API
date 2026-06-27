namespace Domain.Abstractions.Services;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Username { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}

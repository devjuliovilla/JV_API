using Microsoft.EntityFrameworkCore;
using JV_API.Domain.Common;

namespace JV_API.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
    public DbSet<Domain.Entities.Role> Roles => Set<Domain.Entities.Role>();
    public DbSet<Domain.Entities.UserRole> UserRoles => Set<Domain.Entities.UserRole>();
    public DbSet<Domain.Entities.RefreshToken> RefreshTokens => Set<Domain.Entities.RefreshToken>();
    public DbSet<Domain.Entities.Category> Categories => Set<Domain.Entities.Category>();
    public DbSet<Domain.Entities.Product> Products => Set<Domain.Entities.Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

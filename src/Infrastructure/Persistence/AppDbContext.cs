using Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Domain.Common;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
    public DbSet<Domain.Entities.Role> Roles => Set<Domain.Entities.Role>();
    public DbSet<Domain.Entities.UserRole> UserRoles => Set<Domain.Entities.UserRole>();
    public DbSet<Domain.Entities.RefreshToken> RefreshTokens => Set<Domain.Entities.RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

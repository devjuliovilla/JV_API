using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LogEntryConfiguration : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("Logs", "log");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Level).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).IsRequired();
        builder.Property(x => x.Properties).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => x.CreatedAt);
    }
}

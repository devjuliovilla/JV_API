using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using JV_API.Domain.Entities;

namespace JV_API.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "sec");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Token).HasMaxLength(512).IsRequired();
        builder.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId);
        builder.HasQueryFilter(x => !x.Deleted);
    }
}

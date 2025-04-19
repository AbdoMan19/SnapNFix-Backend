using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Token).HasMaxLength(200);
        builder.HasIndex(u => u.Token).IsUnique();
        builder.HasOne(u => u.User)
            .WithOne(u => u.RefreshToken)
            .HasForeignKey<User>(u => u.RefreshTokenId);
    }
}
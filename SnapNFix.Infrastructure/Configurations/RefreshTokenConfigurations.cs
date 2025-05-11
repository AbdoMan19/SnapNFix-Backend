using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties
        builder.Property(u => u.Token).HasMaxLength(200);
        
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();
        
        // Indexes
        builder.HasIndex(u => u.Token).IsUnique();
        
        // Relationships
        builder.HasOne(u => u.UserDevice)
            .WithOne(u => u.RefreshToken)
            .HasForeignKey<UserDevice>(u => u.RefreshTokenId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.PhoneNumber)
            .IsRequired();
            
        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique();
            
        builder.Property(u => u.Email).IsRequired(false);
        builder.Property(u => u.NormalizedEmail).IsRequired(false);

        builder.HasIndex(u => new { u.FirstName, u.LastName });

        builder.HasMany(u => u.SnapReports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.FastReports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.RefreshToken)
            .WithOne(r => r.User)
            .IsRequired(false);
    }
}
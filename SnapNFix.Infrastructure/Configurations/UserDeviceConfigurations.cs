using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class UserDeviceConfigurations : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties
        builder.Property(u => u.DeviceName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.DeviceId)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.Platform)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(u => u.DeviceType)
            .IsRequired()
            .HasMaxLength(50);
            
        // Indexes
        builder.HasIndex(u => new { u.DeviceName, u.DeviceId })
            .IsUnique();
        
        // Relationships
        builder.HasOne(u => u.User)
            .WithMany(u => u.UserDevices)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(u => u.RefreshToken)
            .WithOne(u => u.UserDevice)
            .HasForeignKey<RefreshToken>(u => u.UserDeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
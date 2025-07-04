using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class UserDeviceConfigurations : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.HasKey(u => u.Id);
        
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

        builder.Property(u => u.FCMToken)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.DeviceId)
            .IsUnique()
            .HasDatabaseName("IX_UserDevices_DeviceId");

        builder.HasIndex(u => new { u.UserId, u.LastUsedAt })
            .HasDatabaseName("IX_UserDevices_User_LastUsed");

        builder.HasIndex(u => u.FCMToken)
            .HasDatabaseName("IX_UserDevices_FCMToken")
            .HasFilter("\"FCMToken\" IS NOT NULL AND \"FCMToken\" != ''");

        builder.HasOne(u => u.User)
            .WithMany(u => u.UserDevices)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(u => u.RefreshToken)
            .WithOne(u => u.UserDevice)
            .HasForeignKey<RefreshToken>(u => u.UserDeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("UserDevices");
    }
}
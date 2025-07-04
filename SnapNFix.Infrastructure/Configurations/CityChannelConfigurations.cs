using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class CityChannelConfigurations : IEntityTypeConfiguration<CityChannel>
{
    public void Configure(EntityTypeBuilder<CityChannel> builder)
    {
        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cc => cc.Country)
            .HasMaxLength(50);
        
        builder.Property(cc => cc.State)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasMany(cc => cc.Subscriptions)
            .WithOne(s => s.CityChannel)
            .HasForeignKey(s => s.CityChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cc => new { cc.Name, cc.State })
            .IsUnique()
            .HasDatabaseName("IX_CityChannels_Name_State_Unique");

        builder.HasIndex(cc => cc.IsActive)
            .HasDatabaseName("IX_CityChannels_IsActive");

        builder.HasIndex(cc => new { cc.IsActive, cc.Name })
            .HasDatabaseName("IX_CityChannels_Active_Name");

        builder.ToTable("CityChannels");
    }
}
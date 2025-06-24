using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class CityChannelConfigurations : IEntityTypeConfiguration<CityChannel>
{
    public void Configure(EntityTypeBuilder<CityChannel> builder)
    {
        // Primary Key
        builder.HasKey(cc => cc.Id);

        // Properties
        builder.Property(cc => cc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cc => cc.Country)
            .HasMaxLength(50);
        
        builder.Property(cc => cc.State)
            .IsRequired()
            .HasMaxLength(50);

        // Relationships
        builder.HasMany(cc => cc.Subscriptions)
            .WithOne(s => s.CityChannel)
            .HasForeignKey(s => s.CityChannelId)
            .OnDelete(DeleteBehavior.Cascade);
        
        

        // Indexes
        builder.HasIndex(cc => cc.Name)
            .IsUnique();
    }
}
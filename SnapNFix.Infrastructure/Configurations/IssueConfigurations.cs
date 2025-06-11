using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        // Primary key
        builder.HasKey(i => i.Id);
        
        // Basic properties
        builder.Property(i => i.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.Severity.Unspecified);

        // Location property (PostGIS)
        builder.Property(i => i.Location)
            .IsRequired()
            .HasColumnType("geometry (point, 4326)");

        // Address properties
        builder.Property(i => i.Road)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty);

        builder.Property(i => i.City)
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty);

        builder.Property(i => i.State)
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty);

        builder.Property(i => i.Country)
            .HasMaxLength(100)
            .HasDefaultValue("Egypt");

        // DateTime properties
        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relationships
        builder.HasMany(i => i.AssociatedSnapReports)
            .WithOne(sr => sr.Issue)
            .HasForeignKey(sr => sr.IssueId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.AssociatedFastReports)
            .WithOne(fr => fr.Issue)
            .HasForeignKey(fr => fr.IssueId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Category);
        builder.HasIndex(i => i.City);
        builder.HasIndex(i => i.State);
        builder.HasIndex(i => i.Country);
        builder.HasIndex(i => i.CreatedAt);
        builder.HasIndex(i => i.Severity);
        
        // Spatial index for location
        builder.HasIndex(i => i.Location)
            .HasMethod("gist");

        // Composite indexes for common queries
        builder.HasIndex(i => new { i.Status, i.CreatedAt });
        builder.HasIndex(i => new { i.Category, i.Status });
        builder.HasIndex(i => new { i.City, i.Status });

        // Ignore computed property from database mapping
        builder.Ignore(i => i.FullAddress);

        // Table name
        builder.ToTable("Issues");
    }
}
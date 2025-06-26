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
            .HasDefaultValue(Domain.Enums.Severity.NotSpecified);

        // Location property (PostGIS)
        builder.Property(i => i.Location)
            .IsRequired()
            .HasColumnType("geometry (point, 4326)");

        // Address properties
        builder.Property(i => i.Road)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty)
            .IsRequired(false);

        builder.Property(i => i.City)
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty)
            .IsRequired(false);

        builder.Property(i => i.State)
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty)
            .IsRequired(false);

        builder.Property(i => i.Country)
            .HasMaxLength(100)
            .HasDefaultValue("Egypt")
            .IsRequired(false);

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

        // OPTIMIZED INDEXES
        
        // Most critical indexes for dashboard and filtering
        builder.HasIndex(i => i.Status)
            .HasDatabaseName("IX_Issues_Status");

        builder.HasIndex(i => i.CreatedAt)
            .HasDatabaseName("IX_Issues_CreatedAt");

        // Spatial index for location-based queries
        builder.HasIndex(i => i.Location)
            .HasMethod("gist")
            .HasDatabaseName("IX_Issues_Location");

        // Composite indexes for common admin queries
        
        // Status + time filtering (dashboard metrics)
        builder.HasIndex(i => new { i.Status, i.CreatedAt })
            .HasDatabaseName("IX_Issues_Status_Created");

        // Category + status filtering (statistics)
        builder.HasIndex(i => new { i.Category, i.Status })
            .HasDatabaseName("IX_Issues_Category_Status");

        // Geographic analysis
        builder.HasIndex(i => new { i.State, i.Status })
            .HasDatabaseName("IX_Issues_State_Status")
            .HasFilter("\"State\" IS NOT NULL AND \"State\" != ''");

        // City-specific queries for city channels
        builder.HasIndex(i => new { i.City, i.State, i.Status })
            .HasDatabaseName("IX_Issues_City_State_Status")
            .HasFilter("\"City\" IS NOT NULL AND \"State\" IS NOT NULL");

        // Severity-based queries
        builder.HasIndex(i => new { i.Severity, i.Status })
            .HasDatabaseName("IX_Issues_Severity_Status");

        // Ignore computed property
        builder.Ignore(i => i.FullAddress);

        builder.ToTable("Issues");
    }
}

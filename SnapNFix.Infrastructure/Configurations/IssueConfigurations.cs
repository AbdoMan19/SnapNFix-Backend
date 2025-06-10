using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        //Primary Key
        builder.HasKey(i => i.Id);

        // Relationships
        builder.HasMany(i => i.AssociatedFastReports)
            .WithOne(f => f.Issue)
            .HasForeignKey(f => f.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.AssociatedSnapReports)
            .WithOne(s => s.Issue)
            .HasForeignKey(s => s.IssueId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Properties
        builder.Property(i => i.Status).HasConversion<string>();
        builder.Property(i => i.Category).HasConversion<string>();
        builder.Property(i => i.Severity).HasConversion<string>();
        builder.Property(i => i.ImagePath)
            .IsRequired();
        builder.Property(i => i.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();
        
        
        // Indexes
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Category);
        builder.HasIndex("Location").HasMethod("GIST");
        
        // PostGIS - Location column (geography point)
        builder.Property<Point>("Location")
            .HasColumnType("geography(Point,4326)");

        
    }
}
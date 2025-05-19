using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;
using Point = NetTopologySuite.Geometries.Point;

namespace SnapNFix.Infrastructure.Configurations;

public class SnapReportConfiguration : IEntityTypeConfiguration<SnapReport>
{
    public void Configure(EntityTypeBuilder<SnapReport> builder)
    {
        // Primary Key
        builder.HasKey(r => r.Id);

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.SnapReports)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Issue)
            .WithMany( i => i.AssociatedSnapReports)
            .HasForeignKey(r => r.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Properties
        builder.Property(r => r.ImageStatus).HasConversion<string>();
        builder.Property(r => r.ReportCategory).HasConversion<string>();
        builder.Property(r => r.Comment)
            .HasMaxLength(500)
            .IsRequired(false);
        builder.Property(r => r.ImagePath).IsRequired();
        builder.Property(r => r.TaskId).IsRequired(false);
        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();
        builder.Property(r => r.DeletedAt);
        builder.Property(r => r.Threshold)
            .HasPrecision(5, 2)
            .IsRequired(false);
        

        // Indexes
        builder.HasIndex(fr => new { fr.UserId, fr.IssueId })
            .IsUnique(); 
        builder.HasIndex(r => r.TaskId).IsUnique();
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.IssueId);
        builder.HasIndex(r => r.ImageStatus);
        builder.HasIndex(r => r.Category);
        

        // PostGIS - Location column (geography point)
        builder.Property<Point>("Location")
            .HasColumnType("geography(Point,4326)");

        builder.HasIndex("Location").HasMethod("GIST");
    }
}
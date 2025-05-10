using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;
using Point = NetTopologySuite.Geometries.Point;

namespace SnapNFix.Infrastructure.Configurations;

public class SnapReportConfiguration : IEntityTypeConfiguration<SnapReport>
{
    public void Configure(EntityTypeBuilder<SnapReport> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.User)
            .WithMany(u => u.SnapReports)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Issue)
            .WithMany( i => i.AssociatedSnapReports)
            .HasForeignKey(r => r.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(r => r.ImageStatus).HasConversion<string>();
        builder.Property(r => r.ReportCategory).HasConversion<string>();

        // Indexes
        builder.HasIndex(fr => new { fr.UserId, fr.IssueId })
            .IsUnique(); 
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
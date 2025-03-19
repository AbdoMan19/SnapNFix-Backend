using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class SnapReportConfiguration : IEntityTypeConfiguration<SnapReport>
{
    public void Configure(EntityTypeBuilder<SnapReport> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.User)
            .WithMany(u => u.SnapReports)
            .HasForeignKey(r => r.UserId);

        builder.HasOne(r => r.Issue)
            .WithMany()
            .HasForeignKey(r => r.IssueId);

        builder.Property(r => r.Status).HasConversion<string>();
        builder.Property(r => r.Category).HasConversion<string>();

        // Indexes
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.IssueId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Category);

        // Partial index for IsDeleted = false
        builder.HasIndex(r => r.IssueId)
            .HasFilter("\"IsDeleted\" = FALSE");

        // PostGIS - Location column (geography point)
        builder.Property<Point>("Location")
            .HasColumnType("geography(Point,4326)");

        builder.HasIndex("Location").HasMethod("GIST");
    }
}
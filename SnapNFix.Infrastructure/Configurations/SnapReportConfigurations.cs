using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class SnapReportConfiguration : IEntityTypeConfiguration<SnapReport>
{
    public void Configure(EntityTypeBuilder<SnapReport> builder)
    {
        // Primary key
        builder.HasKey(sr => sr.Id);

        // Basic properties
        builder.Property(sr => sr.Comment)
            .HasMaxLength(1000)
            .HasDefaultValue(string.Empty);

        builder.Property(sr => sr.ImagePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sr => sr.TaskId)
            .HasMaxLength(100);

        builder.Property(sr => sr.Threshold)
            .HasPrecision(5, 4);

        // Enum properties
        builder.Property(sr => sr.ImageStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.ImageStatus.Pending);

        builder.Property(sr => sr.ReportCategory)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.ReportCategory.NotSpecified);

        builder.Property(sr => sr.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.Severity.Unspecified);

        // Location property (PostGIS)
        builder.Property(sr => sr.Location) 
            .IsRequired()
            .HasColumnType("geometry (point, 4326)");

        // Address properties
        builder.Property(sr => sr.Road)
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty)
            .IsRequired(false);

        builder.Property(sr => sr.City)
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty)
            .IsRequired(false);

        builder.Property(sr => sr.State)
            .HasMaxLength(100)
            .HasDefaultValue(string.Empty)
            .IsRequired(false);

        builder.Property(sr => sr.Country)
            .HasMaxLength(100)
            .HasDefaultValue("Egypt")
            .IsRequired(false);
        

        // DateTime properties
        builder.Property(sr => sr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(sr => sr.DeletedAt)
            .IsRequired(false);

        // Foreign keys
        builder.Property(sr => sr.UserId)
            .IsRequired();

        builder.Property(sr => sr.IssueId)
            .IsRequired(false);

        // Relationships
        builder.HasOne(sr => sr.User)
            .WithMany(u => u.SnapReports)
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.Issue)
            .WithMany(i => i.AssociatedSnapReports)
            .HasForeignKey(sr => sr.IssueId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes for performance
        builder.HasIndex(sr => sr.UserId);
        builder.HasIndex(sr => sr.IssueId);
        builder.HasIndex(sr => sr.ImageStatus);
        builder.HasIndex(sr => sr.ReportCategory);
        builder.HasIndex(sr => sr.City);
        builder.HasIndex(sr => sr.State);
        builder.HasIndex(sr => sr.Country);
        builder.HasIndex(sr => sr.CreatedAt);
        builder.HasIndex(sr => sr.DeletedAt);
        builder.HasIndex(sr => sr.Severity);

        // Spatial index for location
        builder.HasIndex(sr => sr.Location)
            .HasMethod("gist");

        // Composite indexes for common filtering scenarios
        builder.HasIndex(sr => new { sr.UserId, sr.CreatedAt });
        builder.HasIndex(sr => new { sr.ImageStatus, sr.CreatedAt });
        builder.HasIndex(sr => new { sr.ReportCategory, sr.ImageStatus });
        builder.HasIndex(sr => new { sr.City, sr.ImageStatus });
        builder.HasIndex(sr => new { sr.DeletedAt, sr.CreatedAt })
            .HasFilter("\"DeletedAt\" IS NULL"); // Partial index for active reports
        builder.HasIndex(sr => new { sr.IssueId, sr.CreatedAt });
        
        // Ignore computed property from database mapping
        builder.Ignore(sr => sr.FullAddress);

        // Table name
        builder.ToTable("SnapReports");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class SnapReportConfiguration : IEntityTypeConfiguration<SnapReport>
{
    public void Configure(EntityTypeBuilder<SnapReport> builder)
    {
        builder.HasKey(sr => sr.Id);

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
            .HasDefaultValue(Domain.Enums.Severity.Low);

        builder.Property(sr => sr.Location) 
            .IsRequired()
            .HasColumnType("geometry (point, 4326)");

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

        builder.Property(sr => sr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(sr => sr.DeletedAt)
            .IsRequired(false);

        builder.Property(sr => sr.UserId)
            .IsRequired();

        builder.Property(sr => sr.IssueId)
            .IsRequired(false);

        builder.HasOne(sr => sr.User)
            .WithMany(u => u.SnapReports)
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.Issue)
            .WithMany(i => i.AssociatedSnapReports)
            .HasForeignKey(sr => sr.IssueId)
            .OnDelete(DeleteBehavior.SetNull);

        
        builder.HasIndex(sr => sr.UserId)
            .HasDatabaseName("IX_SnapReports_UserId");

        builder.HasIndex(sr => sr.IssueId)
            .HasDatabaseName("IX_SnapReports_IssueId")
            .HasFilter("\"IssueId\" IS NOT NULL");

        builder.HasIndex(sr => sr.Location)
            .HasMethod("gist")
            .HasDatabaseName("IX_SnapReports_Location");

        builder.HasIndex(sr => sr.ImageStatus)
            .HasDatabaseName("IX_SnapReports_ImageStatus");

        builder.HasIndex(sr => sr.CreatedAt)
            .HasDatabaseName("IX_SnapReports_CreatedAt");


        builder.HasIndex(sr => new { sr.UserId, sr.ImageStatus, sr.CreatedAt })
            .HasDatabaseName("IX_SnapReports_User_Status_Created");

        builder.HasIndex(sr => new { sr.IssueId, sr.CreatedAt })
            .HasDatabaseName("IX_SnapReports_Issue_Created")
            .HasFilter("\"IssueId\" IS NOT NULL");

        builder.HasIndex(sr => new { sr.City, sr.ImageStatus })
            .HasDatabaseName("IX_SnapReports_City_Status")
            .HasFilter("\"City\" IS NOT NULL AND \"City\" != ''");

        builder.HasIndex(sr => sr.TaskId)
            .HasDatabaseName("IX_SnapReports_TaskId")
            .IsUnique()
            .HasFilter("\"TaskId\" IS NOT NULL");

        builder.HasIndex(sr => new { sr.UserId, sr.IssueId })
            .IsUnique()
            .HasDatabaseName("IX_SnapReports_User_Issue_Unique")
            .HasFilter("\"IssueId\" IS NOT NULL");

        // Ignore computed property
        builder.Ignore(sr => sr.FullAddress);

        builder.ToTable("SnapReports");
    }
}
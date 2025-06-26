using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Infrastructure.Configurations;

public class FastReportConfiguration : IEntityTypeConfiguration<FastReport>
{
    public void Configure(EntityTypeBuilder<FastReport> builder)
    {
        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.User)
            .WithMany(u => u.FastReports)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Issue)
            .WithMany(i => i.AssociatedFastReports)
            .HasForeignKey(f => f.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(f => f.Comment)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(Severity.Low);

        builder.HasIndex(f => f.IssueId)
            .HasDatabaseName("IX_FastReports_IssueId");

        builder.HasIndex(f => f.UserId)
            .HasDatabaseName("IX_FastReports_UserId");

        builder.HasIndex(f => new { f.IssueId, f.CreatedAt })
            .HasDatabaseName("IX_FastReports_Issue_Created");

        builder.HasIndex(f => new { f.UserId, f.CreatedAt })
            .HasDatabaseName("IX_FastReports_User_Created");

        builder.HasIndex(f => new { f.UserId, f.IssueId })
            .IsUnique()
            .HasDatabaseName("IX_FastReports_User_Issue_Unique");

        builder.ToTable("FastReports");
    }
}
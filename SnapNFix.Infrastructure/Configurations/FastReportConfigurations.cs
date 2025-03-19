using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class FastReportConfiguration : IEntityTypeConfiguration<FastReport>
{
    public void Configure(EntityTypeBuilder<FastReport> builder)
    {
        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.User)
            .WithMany(u => u.FastReports)
            .HasForeignKey(f => f.UserId);

        builder.HasOne(f => f.Issue)
            .WithMany(i => i.AssociatedFastReports)
            .HasForeignKey(f => f.IssueId);

        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.IssueId);

        // Partial index for non-deleted (if applicable in future)
        builder.HasIndex(f => f.IssueId)
            .HasFilter("\"IsDeleted\" = FALSE"); // Optional if you add IsDeleted later
    }
}
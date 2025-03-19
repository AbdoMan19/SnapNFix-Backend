using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.HasKey(i => i.Id);

        builder.HasOne(i => i.MainReport)
            .WithOne()
            .HasForeignKey<Issue>(i => i.MainReportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.MainReportId);

        builder.HasMany(i => i.AssociatedFastReports)
            .WithOne(f => f.Issue)
            .HasForeignKey(f => f.IssueId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(i => i.MainReportId); // For FK filtering

        builder.HasIndex(i => i.MainReportId).IsUnique();
    }
}
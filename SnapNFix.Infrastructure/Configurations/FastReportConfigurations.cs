using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Infrastructure.Configurations;

public class FastReportConfiguration : IEntityTypeConfiguration<FastReport>
{
    public void Configure(EntityTypeBuilder<FastReport> builder)
    {
        // Primary Key
        builder.HasKey(f => f.Id);

        // Properties
        builder.HasOne(f => f.User)
            .WithMany(u => u.FastReports)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Issue)
            .WithMany(i => i.AssociatedFastReports)
            .HasForeignKey(f => f.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Properties
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
        
            
        
        // Indexes
        builder.HasIndex(fr => new { fr.UserId, fr.IssueId })
            .IsUnique(); 
        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.IssueId);
        
    }
}
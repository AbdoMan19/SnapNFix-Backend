using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations
{
    public class AdminTargetConfiguration : IEntityTypeConfiguration<AdminTarget>
    {
        public void Configure(EntityTypeBuilder<AdminTarget> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TargetResolutionRate)
                .IsRequired()
                .HasPrecision(5, 2); 

            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            builder.Property(t => t.UpdatedAt)
                .IsRequired();

            builder.HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName("IX_AdminTargets_IsActive");

            builder.ToTable("AdminTargets");
        }
    }
}
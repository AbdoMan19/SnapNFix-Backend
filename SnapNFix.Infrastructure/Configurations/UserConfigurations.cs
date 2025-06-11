using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.PhoneNumber)
            .IsRequired();
            
        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique();
            
        builder.Property(u => u.Email).IsRequired(false);

        builder.Property(u => u.NormalizedEmail).IsRequired(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("date")
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("date")
            .ValueGeneratedOnUpdate();

        builder.Property(u => u.DeletedAt)
            .HasColumnType("date")
            .IsRequired(false);


        builder.Property(u => u.Gender)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Domain.Enums.Gender.NotSpecified);

        builder.Property(u => u.BirthDate)
            .HasColumnType("date")
            .IsRequired(false);
        

        // Indexes
        builder.HasIndex(u => new { u.FirstName, u.LastName });
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.IsDeleted);
        builder.HasIndex(u => u.Gender);
        builder.HasIndex(u => u.DeletedAt);
         builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasFilter("\"DeletedAt\" IS NULL"); // Performance index for name searches
        
        // Relationships
        builder.HasMany(u => u.SnapReports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.FastReports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);


    }
}
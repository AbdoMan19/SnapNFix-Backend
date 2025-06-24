using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.PhoneNumber)
            .IsRequired(false); 
            
        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique()
            .HasFilter("\"PhoneNumber\" IS NOT NULL");
            
        builder.Property(u => u.Email)
            .IsRequired(false)
            .HasMaxLength(256);


        builder.Property(u => u.NormalizedEmail)
            .IsRequired(false)
            .HasMaxLength(256);


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
        
        builder.HasIndex(u => new { u.FirstName, u.LastName });
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.IsDeleted);
        builder.HasIndex(u => u.Gender);
        builder.HasIndex(u => u.DeletedAt);
        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasFilter("\"DeletedAt\" IS NULL");
        
        builder.HasMany(u => u.SnapReports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.FastReports)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(u => u.UserDevices)
            .WithOne(ud => ud.User)
            .HasForeignKey(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(u => u.CityChannelSubscriptions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Ignore(sr => sr.FullName);
    }
}
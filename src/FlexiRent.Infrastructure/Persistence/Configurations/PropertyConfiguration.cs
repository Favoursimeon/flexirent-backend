using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(p => p.Region)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.PricePerMonth)
            .HasPrecision(18, 2);

        builder.Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // PostgreSQL tsvector for full-text search
        builder.Property(p => p.SearchVector)
     .IsRequired(false);

        builder.HasIndex(p => p.SearchVector)
            .HasMethod("GIN");

        builder.HasIndex(p => p.Region);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.OwnerId);

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.Properties)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Property)
            .HasForeignKey(i => i.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Comment).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Rating).IsRequired();

        builder.HasIndex(r => r.PropertyId);
        builder.HasIndex(r => r.AuthorId);
        builder.HasIndex(r => new { r.PropertyId, r.AuthorId }).IsUnique();

        builder.HasOne(r => r.Author)
            .WithMany()
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Votes)
            .WithOne(v => v.Review)
            .HasForeignKey(v => v.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
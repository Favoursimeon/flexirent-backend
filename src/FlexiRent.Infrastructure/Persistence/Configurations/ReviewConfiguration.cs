using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TargetType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Rating)
            .HasAnnotation("CheckConstraint", "Rating BETWEEN 1 AND 5");

        builder.HasIndex(r => new { r.TargetId, r.TargetType });

        builder.HasOne(r => r.Author)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
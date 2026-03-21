using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.HasKey(w => w.Id);

        // Prevent duplicate wishlist entries
        builder.HasIndex(w => new { w.UserId, w.PropertyId })
            .IsUnique();

        builder.HasOne(w => w.User)
            .WithMany(u => u.WishlistItems)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Property)
            .WithMany(p => p.WishlistItems)
            .HasForeignKey(w => w.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
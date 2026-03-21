using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        builder.Property(b => b.CancellationReason)
            .HasMaxLength(500);

        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.ProviderId);
        builder.HasIndex(b => b.Status);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Provider)
            .WithMany(u => u.ProvidedBookings)
            .HasForeignKey(b => b.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Property)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Messages)
            .WithOne(m => m.Booking)
            .HasForeignKey(m => m.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
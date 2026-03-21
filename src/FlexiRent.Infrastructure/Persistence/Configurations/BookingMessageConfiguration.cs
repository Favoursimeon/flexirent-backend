using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class BookingMessageConfiguration : IEntityTypeConfiguration<BookingMessage>
{
    public void Configure(EntityTypeBuilder<BookingMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Body)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(m => m.BookingId);

        builder.HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
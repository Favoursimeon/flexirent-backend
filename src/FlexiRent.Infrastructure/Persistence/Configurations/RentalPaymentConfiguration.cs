using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class RentalPaymentConfiguration : IEntityTypeConfiguration<RentalPayment>
{
    public void Configure(EntityTypeBuilder<RentalPayment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .HasMaxLength(10);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.PaystackReference)
            .HasMaxLength(200);

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.LeaseId);
        builder.HasIndex(p => p.PaystackReference)
            .IsUnique()
            .HasFilter("\"PaystackReference\" IS NOT NULL");

        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Schedule)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.ScheduleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
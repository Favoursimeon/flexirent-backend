using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class RentalLeaseConfiguration : IEntityTypeConfiguration<RentalLease>
{
    public void Configure(EntityTypeBuilder<RentalLease> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.MonthlyAmount)
            .HasPrecision(18, 2);

        builder.Property(l => l.Currency)
            .HasMaxLength(10);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(l => l.TenantId);
        builder.HasIndex(l => l.PropertyId);
        builder.HasIndex(l => l.Status);

        builder.HasOne(l => l.Tenant)
            .WithMany(u => u.Leases)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Property)
            .WithMany(p => p.Leases)
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.PaymentSchedules)
            .WithOne(s => s.Lease)
            .HasForeignKey(s => s.LeaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Payments)
            .WithOne(p => p.Lease)
            .HasForeignKey(p => p.LeaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
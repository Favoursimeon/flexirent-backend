using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class PaymentScheduleConfiguration : IEntityTypeConfiguration<PaymentSchedule>
{
    public void Configure(EntityTypeBuilder<PaymentSchedule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Amount)
            .HasPrecision(18, 2);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(s => s.LeaseId);
        builder.HasIndex(s => s.DueDate);
        builder.HasIndex(s => s.Status);
    }
}
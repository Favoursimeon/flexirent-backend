using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class ProviderAvailabilityConfiguration : IEntityTypeConfiguration<ProviderAvailability>
{
    public void Configure(EntityTypeBuilder<ProviderAvailability> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.ProviderId, a.DayOfWeek })
            .IsUnique();

        builder.HasOne(a => a.Provider)
            .WithMany(u => u.Availabilities)
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
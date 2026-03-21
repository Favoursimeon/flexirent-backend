using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class ViewingScheduleConfiguration : IEntityTypeConfiguration<ViewingSchedule>
{
    public void Configure(EntityTypeBuilder<ViewingSchedule> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(v => v.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(v => v.UserId);
        builder.HasIndex(v => v.PropertyId);

        builder.HasOne(v => v.User)
            .WithMany(u => u.ViewingSchedules)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Property)
            .WithMany(p => p.ViewingSchedules)
            .HasForeignKey(v => v.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
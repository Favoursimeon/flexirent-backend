using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class PortfolioImageConfiguration : IEntityTypeConfiguration<PortfolioImage>
{
    public void Configure(EntityTypeBuilder<PortfolioImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Title).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Description).HasMaxLength(1000);
        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(1000);
        builder.Property(i => i.ContentType).IsRequired().HasMaxLength(100);

        builder.HasIndex(i => i.OwnerId);

        builder.HasOne(i => i.Owner)
            .WithMany()
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
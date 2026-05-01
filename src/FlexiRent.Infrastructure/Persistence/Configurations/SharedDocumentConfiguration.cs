using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class SharedDocumentConfiguration : IEntityTypeConfiguration<SharedDocument>
{
    public void Configure(EntityTypeBuilder<SharedDocument> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.SharedWithUserId);
        builder.HasIndex(s => s.SharedByUserId);
        builder.HasIndex(s => new { s.DocumentId, s.SharedWithUserId }).IsUnique();

        builder.HasOne(s => s.SharedByUser)
            .WithMany()
            .HasForeignKey(s => s.SharedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SharedWithUser)
            .WithMany()
            .HasForeignKey(s => s.SharedWithUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
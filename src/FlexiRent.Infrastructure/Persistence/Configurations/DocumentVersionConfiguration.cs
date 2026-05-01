using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.FileUrl).IsRequired().HasMaxLength(1000);
        builder.Property(v => v.FileName).IsRequired().HasMaxLength(500);
        builder.Property(v => v.ChangeNotes).HasMaxLength(1000);

        builder.HasIndex(v => v.DocumentId);

        builder.HasOne(v => v.UploadedBy)
            .WithMany()
            .HasForeignKey(v => v.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
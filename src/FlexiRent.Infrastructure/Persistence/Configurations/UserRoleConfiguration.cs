using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(r => new { r.UserId, r.Role })
            .IsUnique();
    }
}
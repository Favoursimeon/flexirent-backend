using FlexiRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexiRent.Infrastructure.Persistence.Configurations;

public class PaymentAccountConfiguration : IEntityTypeConfiguration<PaymentAccount>
{
    public void Configure(EntityTypeBuilder<PaymentAccount> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AccountNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.BankName)
            .HasMaxLength(100);

        builder.Property(a => a.MobileProvider)
            .HasMaxLength(50);

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(a => a.UserId);

        builder.HasOne(a => a.User)
            .WithMany(u => u.PaymentAccounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
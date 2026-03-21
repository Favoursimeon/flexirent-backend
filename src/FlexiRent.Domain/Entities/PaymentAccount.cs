using FlexiRent.Domain.Enums;

namespace FlexiRent.Domain.Entities;

public class PaymentAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public PaymentAccountType Type { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? MobileProvider { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
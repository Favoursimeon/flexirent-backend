using FlexiRent.Domain.Enums;

namespace FlexiRent.Application.DTOs;

public class InitiatePaymentDto
{
    public Guid LeaseId { get; set; }
    public Guid? ScheduleId { get; set; }
    public decimal Amount { get; set; }
}

public class InitiatePaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeaseId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? Reference { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLeaseDto
{
    public Guid PropertyId { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string Currency { get; set; } = "GHS";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int PaymentDayOfMonth { get; set; } = 1;
}

public class RentalLeaseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaseStatus Status { get; set; }
    public int PaymentDayOfMonth { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePaymentAccountDto
{
    public PaymentAccountType Type { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? MobileProvider { get; set; }
    public bool IsDefault { get; set; }
}

public class PaymentAccountDto
{
    public Guid Id { get; set; }
    public PaymentAccountType Type { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? MobileProvider { get; set; }
    public bool IsDefault { get; set; }
}
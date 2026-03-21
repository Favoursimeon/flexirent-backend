using FlexiRent.Domain.Enums;

namespace FlexiRent.Domain.Entities;

public class RentalPayment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeaseId { get; set; }
    public Guid? ScheduleId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "GHS";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? PaystackReference { get; set; }
    public string? PaystackTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public bool IsAdminApproved { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public RentalLease Lease { get; set; } = null!;
    public PaymentSchedule? Schedule { get; set; }
}
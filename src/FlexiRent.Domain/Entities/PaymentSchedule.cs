using FlexiRent.Domain.Enums;

namespace FlexiRent.Domain.Entities;

public class PaymentSchedule
{
    public Guid Id { get; set; }
    public Guid LeaseId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public PaymentScheduleStatus Status { get; set; } = PaymentScheduleStatus.Upcoming;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public RentalLease Lease { get; set; } = null!;
    public ICollection<RentalPayment> Payments { get; set; } = new List<RentalPayment>();
}
using FlexiRent.Domain.Enums;

namespace FlexiRent.Domain.Entities;

public class RentalLease
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PropertyId { get; set; }
    public decimal MonthlyAmount { get; set; }
    public string Currency { get; set; } = "GHS";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaseStatus Status { get; set; } = LeaseStatus.Active;
    public int PaymentDayOfMonth { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User Tenant { get; set; } = null!;
    public Property Property { get; set; } = null!;
    public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
    public ICollection<RentalPayment> Payments { get; set; } = new List<RentalPayment>();
}
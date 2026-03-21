using FlexiRent.Domain.Enums;
using NpgsqlTypes;

namespace FlexiRent.Domain.Entities;

public class Property
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerMonth { get; set; }
    public string Region { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Pending;
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double AreaSqft { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Full-text search vector (PostgreSQL tsvector)
    public NpgsqlTsVector? SearchVector { get; set; }
    // Navigation
    public User Owner { get; set; } = null!;
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<ViewingSchedule> ViewingSchedules { get; set; } = new List<ViewingSchedule>();
    public ICollection<RentalLease> Leases { get; set; } = new List<RentalLease>();
}
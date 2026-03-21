using FlexiRent.Domain.Enums;

namespace FlexiRent.Application.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid PropertyId { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBookingDto
{
    public Guid ProviderId { get; set; }
    public Guid PropertyId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateViewingDto
{
    public Guid PropertyId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Notes { get; set; }
}

public class ViewingDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public ViewingStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateBookingStatusDto
{
    public BookingStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class ProviderAvailabilityDto
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public class UpsertAvailabilityDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }
}

public class BookingFilterDto
{
    public BookingStatus? Status { get; set; }
    public Guid? PropertyId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int PageSize { get; set; } = 20;
    public Guid? Cursor { get; set; }
}

public class CancelBookingDto
{
    public string Reason { get; set; } = string.Empty;
}
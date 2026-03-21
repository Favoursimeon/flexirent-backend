using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Domain.Enums;
using FlexiRent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlexiRent.Infrastructure.Services;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid userId, CreateBookingDto dto);
    Task<ViewingDto> CreateViewingAsync(Guid userId, CreateViewingDto dto);
    Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid requesterId, UpdateBookingStatusDto dto);
    Task<BookingDto> CancelBookingAsync(Guid bookingId, Guid requesterId, CancelBookingDto dto);
    Task<PagedResult<BookingDto>> GetBookingsAsync(Guid userId, BookingFilterDto filter);
    Task<PagedResult<ViewingDto>> GetViewingsAsync(Guid userId, int pageSize, Guid? cursor);
    Task<List<ProviderAvailabilityDto>> GetAvailabilityAsync(Guid providerId);
    Task<ProviderAvailabilityDto> UpsertAvailabilityAsync(Guid providerId, UpsertAvailabilityDto dto);
    Task DeleteAvailabilityAsync(Guid availabilityId, Guid providerId);
}

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public BookingService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<BookingDto> CreateBookingAsync(Guid userId, CreateBookingDto dto)
    {
        if (dto.StartAt >= dto.EndAt)
            throw new ApplicationException("Start time must be before end time.");

        if (dto.StartAt < DateTime.UtcNow)
            throw new ApplicationException("Booking cannot be in the past.");

        var property = await _db.Properties
            .FirstOrDefaultAsync(p => p.Id == dto.PropertyId && p.IsAvailable)
            ?? throw new ApplicationException("Property not found or unavailable.");

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProviderId = dto.ProviderId,
            PropertyId = dto.PropertyId,
            Status = BookingStatus.Pending,
            StartAt = dto.StartAt,
            EndAt = dto.EndAt,
            Notes = dto.Notes,
            TotalAmount = CalculateAmount(property.PricePerMonth, dto.StartAt, dto.EndAt),
            CreatedAt = DateTime.UtcNow
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<ViewingDto> CreateViewingAsync(Guid userId, CreateViewingDto dto)
    {
        if (dto.ScheduledAt < DateTime.UtcNow)
            throw new ApplicationException("Viewing cannot be scheduled in the past.");

        var property = await _db.Properties
            .FirstOrDefaultAsync(p => p.Id == dto.PropertyId)
            ?? throw new ApplicationException("Property not found.");

        var viewing = new ViewingSchedule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PropertyId = dto.PropertyId,
            ScheduledAt = dto.ScheduledAt,
            Notes = dto.Notes,
            Status = ViewingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.ViewingSchedules.Add(viewing);
        await _db.SaveChangesAsync();

        return MapViewingToDto(viewing);
    }

    public async Task<BookingDto> UpdateStatusAsync(
    Guid bookingId, Guid requesterId, UpdateBookingStatusDto dto)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new ApplicationException("Booking not found.");

        if (dto.Status is BookingStatus.Confirmed or BookingStatus.Rejected)
        {
            if (booking.ProviderId != requesterId)
                throw new ApplicationException(
                    "Only the provider can confirm or reject bookings.");
        }

        ValidateStatusTransition(booking.Status, dto.Status);

        booking.Status = dto.Status;
        booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Notify the user
        var (title, message, type) = dto.Status switch
        {
            BookingStatus.Confirmed => (
                "Booking Confirmed",
                "Your booking has been confirmed.",
                NotificationType.BookingConfirmed),
            BookingStatus.Rejected => (
                "Booking Rejected",
                "Your booking request was rejected.",
                NotificationType.BookingRejected),
            BookingStatus.Cancelled => (
                "Booking Cancelled",
                "Your booking has been cancelled.",
                NotificationType.BookingCancelled),
            _ => (null, null, NotificationType.General)
        };

        if (title is not null)
        {
            await _notificationService.SendAsync(
                booking.UserId,
                type,
                title,
                message!,
                actionUrl: $"/bookings/{booking.Id}",
                referenceId: booking.Id);
        }

        return MapToDto(booking);
    }

    public async Task<BookingDto> CancelBookingAsync(
        Guid bookingId,
        Guid requesterId,
        CancelBookingDto dto)
    {
        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new ApplicationException("Booking not found.");

        if (booking.UserId != requesterId && booking.ProviderId != requesterId)
            throw new ApplicationException("You are not authorised to cancel this booking.");

        if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
            throw new ApplicationException($"Cannot cancel a {booking.Status} booking.");

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = dto.Reason;
        booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(booking);
    }

    public async Task<PagedResult<BookingDto>> GetBookingsAsync(Guid userId, BookingFilterDto filter)
    {
        var query = _db.Bookings
            .Where(b => b.UserId == userId || b.ProviderId == userId)
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        if (filter.PropertyId.HasValue)
            query = query.Where(b => b.PropertyId == filter.PropertyId.Value);

        if (filter.From.HasValue)
            query = query.Where(b => b.StartAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(b => b.EndAt <= filter.To.Value);

        if (filter.Cursor.HasValue)
        {
            var cursorDate = await _db.Bookings
                .Where(b => b.Id == filter.Cursor.Value)
                .Select(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(b => b.CreatedAt < cursorDate);
        }

        query = query.OrderByDescending(b => b.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Take(filter.PageSize + 1).ToListAsync();
        var hasMore = items.Count > filter.PageSize;

        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<BookingDto>
        {
            Items = items.Select(MapToDto).ToList(),
            NextCursor = hasMore ? items.Last().Id : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    public async Task<PagedResult<ViewingDto>> GetViewingsAsync(
        Guid userId,
        int pageSize,
        Guid? cursor)
    {
        var query = _db.ViewingSchedules
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.ScheduledAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorDate = await _db.ViewingSchedules
                .Where(v => v.Id == cursor.Value)
                .Select(v => v.ScheduledAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(v => v.ScheduledAt < cursorDate);
        }

        var total = await query.CountAsync();
        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;

        if (hasMore) items.RemoveAt(items.Count - 1);

        return new PagedResult<ViewingDto>
        {
            Items = items.Select(MapViewingToDto).ToList(),
            NextCursor = hasMore ? items.Last().Id : null,
            HasMore = hasMore,
            TotalCount = total
        };
    }

    public async Task<List<ProviderAvailabilityDto>> GetAvailabilityAsync(Guid providerId)
    {
        return await _db.ProviderAvailabilities
            .Where(a => a.ProviderId == providerId)
            .OrderBy(a => a.DayOfWeek)
            .Select(a => new ProviderAvailabilityDto
            {
                Id = a.Id,
                DayOfWeek = a.DayOfWeek,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                IsAvailable = a.IsAvailable
            })
            .ToListAsync();
    }

    public async Task<ProviderAvailabilityDto> UpsertAvailabilityAsync(
        Guid providerId,
        UpsertAvailabilityDto dto)
    {
        var existing = await _db.ProviderAvailabilities
            .FirstOrDefaultAsync(a =>
                a.ProviderId == providerId &&
                a.DayOfWeek == dto.DayOfWeek);

        if (existing is not null)
        {
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.IsAvailable = dto.IsAvailable;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new ProviderAvailability
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAvailable = dto.IsAvailable,
                CreatedAt = DateTime.UtcNow
            };
            _db.ProviderAvailabilities.Add(existing);
        }

        await _db.SaveChangesAsync();

        return new ProviderAvailabilityDto
        {
            Id = existing.Id,
            DayOfWeek = existing.DayOfWeek,
            StartTime = existing.StartTime,
            EndTime = existing.EndTime,
            IsAvailable = existing.IsAvailable
        };
    }

    public async Task DeleteAvailabilityAsync(Guid availabilityId, Guid providerId)
    {
        var availability = await _db.ProviderAvailabilities
            .FirstOrDefaultAsync(a => a.Id == availabilityId && a.ProviderId == providerId)
            ?? throw new ApplicationException("Availability slot not found.");

        _db.ProviderAvailabilities.Remove(availability);
        await _db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static decimal CalculateAmount(decimal pricePerMonth, DateTime start, DateTime end)
    {
        var days = (decimal)(end - start).TotalDays;
        return Math.Round(pricePerMonth / 30 * days, 2);
    }

    private static void ValidateStatusTransition(BookingStatus current, BookingStatus next)
    {
        var allowed = new Dictionary<BookingStatus, List<BookingStatus>>
        {
            { BookingStatus.Pending, new() { BookingStatus.Confirmed, BookingStatus.Rejected, BookingStatus.Cancelled } },
            { BookingStatus.Confirmed, new() { BookingStatus.Completed, BookingStatus.Cancelled } },
            { BookingStatus.Rejected, new() { } },
            { BookingStatus.Cancelled, new() { } },
            { BookingStatus.Completed, new() { } }
        };

        if (!allowed[current].Contains(next))
            throw new ApplicationException(
                $"Cannot transition booking from {current} to {next}.");
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        UserId = b.UserId,
        ProviderId = b.ProviderId,
        PropertyId = b.PropertyId,
        Status = b.Status,
        Notes = b.Notes,
        StartAt = b.StartAt,
        EndAt = b.EndAt,
        TotalAmount = b.TotalAmount,
        CancellationReason = b.CancellationReason,
        CreatedAt = b.CreatedAt
    };

    private static ViewingDto MapViewingToDto(ViewingSchedule v) => new()
    {
        Id = v.Id,
        UserId = v.UserId,
        PropertyId = v.PropertyId,
        Status = v.Status,
        ScheduledAt = v.ScheduledAt,
        Notes = v.Notes,
        CreatedAt = v.CreatedAt
    };
}
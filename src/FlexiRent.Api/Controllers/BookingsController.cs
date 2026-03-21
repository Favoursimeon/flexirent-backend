using FlexiRent.Application.DTOs;
using FlexiRent.Domain.Entities;
using FlexiRent.Infrastructure;
using FlexiRent.Infrastructure.Persistence;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserService _currentUser;

    public BookingsController(IBookingService bookingService, ICurrentUserService currentUser)
    {
        _bookingService = bookingService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var result = await _bookingService.CreateBookingAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(GetBookings), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetBookings([FromQuery] BookingFilterDto filter)
    {
        var result = await _bookingService.GetBookingsAsync(_currentUser.UserId, filter);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusDto dto)
    {
        var result = await _bookingService.UpdateStatusAsync(id, _currentUser.UserId, dto);
        return Ok(result);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingDto dto)
    {
        var result = await _bookingService.CancelBookingAsync(id, _currentUser.UserId, dto);
        return Ok(result);
    }

    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(
    Guid id,
    [FromServices] AppDbContext db,
    [FromQuery] int pageSize = 50,
    [FromQuery] Guid? cursor = null)
    {
        var userId = _currentUser.UserId;

        var booking = await db.Bookings
            .FirstOrDefaultAsync(b =>
                b.Id == id &&
                (b.UserId == userId || b.ProviderId == userId));

        if (booking is null) return Forbid();

        var query = db.BookingMessages
            .Where(m => m.BookingId == id)
            .OrderByDescending(m => m.SentAt)
            .AsQueryable();

        if (cursor.HasValue)
        {
            var cursorDate = await db.BookingMessages
                .Where(m => m.Id == cursor.Value)
                .Select(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (cursorDate != default)
                query = query.Where(m => m.SentAt < cursorDate);
        }

        var items = await query.Take(pageSize + 1).ToListAsync();
        var hasMore = items.Count > pageSize;
        if (hasMore) items.RemoveAt(items.Count - 1);

        return Ok(new
        {
            items = items.Select(m => new
            {
                id = m.Id,
                bookingId = m.BookingId,
                senderId = m.SenderId,
                body = m.Body,
                isRead = m.IsRead,
                sentAt = m.SentAt
            }),
            nextCursor = hasMore ? items.Last().Id : (Guid?)null,
            hasMore
        });
    }
}
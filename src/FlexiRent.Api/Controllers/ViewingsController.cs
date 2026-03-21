using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/viewings")]
[Authorize]
public class ViewingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserService _currentUser;

    public ViewingsController(IBookingService bookingService, ICurrentUserService currentUser)
    {
        _bookingService = bookingService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> CreateViewing([FromBody] CreateViewingDto dto)
    {
        var result = await _bookingService.CreateViewingAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(GetViewings), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetViewings(
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? cursor = null)
    {
        var result = await _bookingService.GetViewingsAsync(_currentUser.UserId, pageSize, cursor);
        return Ok(result);
    }
}
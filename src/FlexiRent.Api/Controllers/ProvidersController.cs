using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Authorization;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/providers")]
[Authorize]
public class ProvidersController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserService _currentUser;

    public ProvidersController(IBookingService bookingService, ICurrentUserService currentUser)
    {
        _bookingService = bookingService;
        _currentUser = currentUser;
    }

    [HttpGet("{providerId}/availability")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailability(Guid providerId)
    {
        var result = await _bookingService.GetAvailabilityAsync(providerId);
        return Ok(result);
    }

    [HttpPut("availability")]
    [Authorize(Policy = PolicyConstants.RequireServiceProvider)]
    public async Task<IActionResult> UpsertAvailability([FromBody] UpsertAvailabilityDto dto)
    {
        var result = await _bookingService.UpsertAvailabilityAsync(_currentUser.UserId, dto);
        return Ok(result);
    }

    [HttpDelete("availability/{id}")]
    [Authorize(Policy = PolicyConstants.RequireServiceProvider)]
    public async Task<IActionResult> DeleteAvailability(Guid id)
    {
        await _bookingService.DeleteAvailabilityAsync(id, _currentUser.UserId);
        return NoContent();
    }
}
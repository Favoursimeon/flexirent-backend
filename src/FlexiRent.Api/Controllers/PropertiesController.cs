using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Authorization;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly ICurrentUserService _currentUser;

    public PropertiesController(IPropertyService propertyService, ICurrentUserService currentUser)
    {
        _propertyService = propertyService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PropertySearchDto search)
    {
        var result = await _propertyService.GetAllAsync(search);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] PropertySearchDto search)
    {
        var result = await _propertyService.SearchAsync(search);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine([FromQuery] int pageSize = 20, [FromQuery] Guid? cursor = null)
    {
        var result = await _propertyService.GetByOwnerAsync(_currentUser.UserId, pageSize, cursor);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _propertyService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromForm] CreatePropertyDto dto,
        [FromForm] List<IFormFile>? images)
    {
        var result = await _propertyService.CreateAsync(_currentUser.UserId, dto, images);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePropertyDto dto)
    {
        var result = await _propertyService.UpdateAsync(id, _currentUser.UserId, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isAdmin = _currentUser.IsInRole("Admin");
        await _propertyService.DeleteAsync(id, _currentUser.UserId, isAdmin);
        return NoContent();
    }

    // ── Wishlist ──────────────────────────────────────────────────────────────

    [HttpPost("wishlist/{propertyId}")]
    [Authorize]
    public async Task<IActionResult> AddToWishlist(Guid propertyId)
    {
        await _propertyService.AddToWishlistAsync(_currentUser.UserId, propertyId);
        return NoContent();
    }

    [HttpDelete("wishlist/{propertyId}")]
    [Authorize]
    public async Task<IActionResult> RemoveFromWishlist(Guid propertyId)
    {
        await _propertyService.RemoveFromWishlistAsync(_currentUser.UserId, propertyId);
        return NoContent();
    }

    [HttpGet("wishlist")]
    [Authorize]
    public async Task<IActionResult> GetWishlist(
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? cursor = null)
    {
        var result = await _propertyService.GetWishlistAsync(_currentUser.UserId, pageSize, cursor);
        return Ok(result);
    }

    [HttpPut("properties/{id}/status")]
    [Authorize(Policy = PolicyConstants.RequireAdminOrModerator)]
    public async Task<IActionResult> UpdatePropertyStatus(
    Guid id,
    [FromBody] UpdatePropertyStatusDto dto)
    {
        await _propertyService.UpdateStatusAsync(id, dto);
        return NoContent();
    }
}
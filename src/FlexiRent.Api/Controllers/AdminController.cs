using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Authorization;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = PolicyConstants.RequireAdminOrModerator)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ICurrentUserService _currentUser;

    public AdminController(IAdminService adminService, ICurrentUserService currentUser)
    {
        _adminService = adminService;
        _currentUser = currentUser;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? cursor = null)
    {
        var result = await _adminService.GetUsersAsync(pageSize, cursor);
        return Ok(result);
    }

    [HttpPut("users/{id}")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserDto dto)
    {
        var result = await _adminService.UpdateUserAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("users/{id}")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _adminService.DeleteUserAsync(id);
        return NoContent();
    }

    // ── Approvals ─────────────────────────────────────────────────────────────

    [HttpPut("vendors/{id}/approve")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> ApproveVendor(Guid id)
    {
        await _adminService.ApproveVendorAsync(id);
        return NoContent();
    }

    [HttpPut("providers/{id}/approve")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> ApproveServiceProvider(Guid id)
    {
        await _adminService.ApproveServiceProviderAsync(id);
        return NoContent();
    }

    [HttpPut("properties/{id}/status")]
    public async Task<IActionResult> UpdatePropertyStatus(
        Guid id,
        [FromBody] UpdatePropertyStatusDto dto,
        [FromServices] IPropertyService propertyService)
    {
        await propertyService.UpdateStatusAsync(id, dto);
        return NoContent();
    }

    // ── Payments ──────────────────────────────────────────────────────────────

    [HttpPut("payments/{id}/approve")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> ApprovePayment(
        Guid id,
        [FromServices] IPaymentService paymentService)
    {
        await paymentService.ApprovePaymentAsync(id);
        return NoContent();
    }

    // ── Reviews ───────────────────────────────────────────────────────────────

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews(
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? cursor = null)
    {
        var result = await _adminService.GetReviewsAsync(pageSize, cursor);
        return Ok(result);
    }

    [HttpPut("reviews/{id}")]
    public async Task<IActionResult> UpdateReview(
        Guid id,
        [FromBody] bool isHidden)
    {
        await _adminService.UpdateReviewAsync(id, isHidden);
        return NoContent();
    }

    // ── Verifications ─────────────────────────────────────────────────────────

    [HttpGet("verifications")]
    public async Task<IActionResult> GetVerifications(
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? cursor = null)
    {
        var result = await _adminService.GetVerificationsAsync(pageSize, cursor);
        return Ok(result);
    }

    [HttpPut("verifications/{userId}")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> UpdateVerification(
        Guid userId,
        [FromBody] AdminVerificationUpdateDto dto)
    {
        await _adminService.UpdateVerificationAsync(userId, dto);
        return NoContent();
    }

    // ── Currencies ────────────────────────────────────────────────────────────

    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies()
    {
        var result = await _adminService.GetCurrenciesAsync();
        return Ok(result);
    }

    [HttpPost("currencies")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyDto dto)
    {
        var result = await _adminService.CreateCurrencyAsync(dto);
        return CreatedAtAction(nameof(GetCurrencies), result);
    }

    [HttpPut("currencies/{id}")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> UpdateCurrency(Guid id, [FromBody] CreateCurrencyDto dto)
    {
        var result = await _adminService.UpdateCurrencyAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("currencies/{id}")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> DeleteCurrency(Guid id)
    {
        await _adminService.DeleteCurrencyAsync(id);
        return NoContent();
    }

    // ── Reports & Analytics ───────────────────────────────────────────────────

    [HttpGet("reports")]
    public async Task<IActionResult> GetFinancialReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _adminService.GetFinancialReportAsync(from, to);
        return Ok(result);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var result = await _adminService.GetAnalyticsAsync();
        return Ok(result);
    }

    // ── FlexiScore ────────────────────────────────────────────────────────────

    [HttpPut("users/{id}/flexi-score/override")]
    [Authorize(Policy = PolicyConstants.RequireAdmin)]
    public async Task<IActionResult> OverrideFlexiScore(
        Guid id,
        [FromBody] FlexiScoreOverrideDto dto)
    {
        await _adminService.OverrideFlexiScoreAsync(id, dto);
        return NoContent();
    }
}
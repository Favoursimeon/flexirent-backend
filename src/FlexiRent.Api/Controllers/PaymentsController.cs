using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Authorization;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUser;

    public PaymentsController(IPaymentService paymentService, ICurrentUserService currentUser)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
    }

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDto dto)
    {
        var result = await _paymentService.InitiatePaymentAsync(_currentUser.UserId, dto);
        return Ok(result);
    }

    [HttpGet("{reference}/verify")]
    public async Task<IActionResult> VerifyPayment(string reference)
    {
        var result = await _paymentService.VerifyPaymentAsync(reference);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? cursor = null)
    {
        var result = await _paymentService
            .GetPaymentHistoryAsync(_currentUser.UserId, pageSize, cursor);
        return Ok(result);
    }

    [HttpGet("history/export")]
    public async Task<IActionResult> ExportHistory()
    {
        var csv = await _paymentService.ExportPaymentHistoryCsvAsync(_currentUser.UserId);
        return File(csv, "text/csv", $"payments-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
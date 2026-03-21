using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/payment-accounts")]
[Authorize]
public class PaymentAccountsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUser;

    public PaymentAccountsController(
        IPaymentService paymentService,
        ICurrentUserService currentUser)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts()
    {
        var result = await _paymentService.GetPaymentAccountsAsync(_currentUser.UserId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAccount([FromBody] CreatePaymentAccountDto dto)
    {
        var result = await _paymentService
            .AddPaymentAccountAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(GetAccounts), result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        await _paymentService.DeletePaymentAccountAsync(id, _currentUser.UserId);
        return NoContent();
    }
}
using FlexiRent.Application.DTOs;
using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/leases")]
[Authorize]
public class LeasesController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUser;

    public LeasesController(IPaymentService paymentService, ICurrentUserService currentUser)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLease([FromBody] CreateLeaseDto dto)
    {
        var result = await _paymentService.CreateLeaseAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(RenewLease), new { id = result.Id }, result);
    }

    [HttpPost("{id}/renew")]
    public async Task<IActionResult> RenewLease(Guid id)
    {
        var result = await _paymentService.RenewLeaseAsync(id, _currentUser.UserId);
        return Ok(result);
    }
}
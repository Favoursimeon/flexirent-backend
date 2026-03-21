using FlexiRent.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlexiRent.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public WebhooksController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("paystack")]
    public async Task<IActionResult> PaystackWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        var signature = Request.Headers["x-paystack-signature"].ToString();

        await _paymentService.HandleWebhookAsync(payload, signature);
        return Ok();
    }
}
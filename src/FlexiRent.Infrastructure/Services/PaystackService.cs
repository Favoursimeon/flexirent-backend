using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FlexiRent.Infrastructure.Services;

public interface IPaystackService
{
    Task<PaystackInitResponse> InitializeTransactionAsync(
        string email, decimal amount, string reference, string callbackUrl);
    Task<PaystackVerifyResponse> VerifyTransactionAsync(string reference);
    bool VerifyWebhookSignature(string payload, string signature);
}

public class PaystackInitResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}

public class PaystackVerifyResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}

public class PaystackService : IPaystackService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public PaystackService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;

        var secretKey = _config["Paystack:SecretKey"]
            ?? throw new InvalidOperationException("Paystack:SecretKey is not configured.");

        _http.BaseAddress = new Uri(
            _config["Paystack:BaseUrl"] ?? "https://api.paystack.co");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", secretKey);
        _http.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<PaystackInitResponse> InitializeTransactionAsync(
        string email,
        decimal amount,
        string reference,
        string callbackUrl)
    {
        // Paystack expects amount in kobo/pesewas (multiply by 100)
        var payload = new
        {
            email,
            amount = (long)(amount * 100),
            reference,
            callback_url = callbackUrl,
            currency = "GHS"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("/transaction/initialize", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Paystack init failed: {responseBody}");

        var doc = JsonDocument.Parse(responseBody);
        var data = doc.RootElement.GetProperty("data");

        return new PaystackInitResponse
        {
            Status = doc.RootElement.GetProperty("status").GetBoolean(),
            Message = doc.RootElement.GetProperty("message").GetString() ?? string.Empty,
            AuthorizationUrl = data.GetProperty("authorization_url").GetString() ?? string.Empty,
            AccessCode = data.GetProperty("access_code").GetString() ?? string.Empty,
            Reference = data.GetProperty("reference").GetString() ?? string.Empty
        };
    }

    public async Task<PaystackVerifyResponse> VerifyTransactionAsync(string reference)
    {
        var response = await _http.GetAsync($"/transaction/verify/{reference}");
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new ApplicationException($"Paystack verify failed: {responseBody}");

        var doc = JsonDocument.Parse(responseBody);
        var data = doc.RootElement.GetProperty("data");

        return new PaystackVerifyResponse
        {
            Status = doc.RootElement.GetProperty("status").GetBoolean(),
            Message = doc.RootElement.GetProperty("message").GetString() ?? string.Empty,
            TransactionStatus = data.GetProperty("status").GetString() ?? string.Empty,
            Reference = data.GetProperty("reference").GetString() ?? string.Empty,
            Amount = data.GetProperty("amount").GetDecimal() / 100,
            Currency = data.GetProperty("currency").GetString() ?? string.Empty,
            TransactionId = data.GetProperty("id").GetInt64().ToString()
        };
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        var webhookSecret = _config["Paystack:WebhookSecret"]
            ?? throw new InvalidOperationException("Paystack:WebhookSecret is not configured.");

        var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA512(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var computedSignature = Convert.ToHexString(hash).ToLower();

        return computedSignature == signature;
    }
}
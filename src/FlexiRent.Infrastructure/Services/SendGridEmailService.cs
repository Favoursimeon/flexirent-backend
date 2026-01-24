using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace FlexiRent.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly SendGridClient _client;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
            var apiKey = _config.GetValue<string>("SendGrid:ApiKey");
            _client = new SendGridClient(apiKey);
            _fromEmail = _config.GetValue<string>("SendGrid:FromEmail") ?? "no-reply@flexirent.example";
            _fromName = _config.GetValue<string>("SendGrid:FromName") ?? "FlexiRent";
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_fromEmail, _fromName),
                Subject = subject,
                HtmlContent = body,
                PlainTextContent = body
            };
            msg.AddTo(new EmailAddress(to));
            var response = await _client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"SendGrid send failed: {response.StatusCode} - {text}");
            }
        }
    }
}
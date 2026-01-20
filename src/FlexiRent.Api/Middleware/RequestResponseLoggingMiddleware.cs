using System.Text;

namespace FlexiRent.Api.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next; _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Minimal request logging
            _logger.LogInformation("HTTP {Method} {Path}", context.Request.Method, context.Request.Path);
            await _next(context);
        }
    }
}

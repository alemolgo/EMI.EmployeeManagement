using System.Diagnostics;

namespace EMI.EmployeeManagement.API.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Incoming Request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );

            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "Response completed in {ElapsedMilliseconds} ms with status code {StatusCode}",
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode
            );
        }
    }
}
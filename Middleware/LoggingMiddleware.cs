using System.Diagnostics;

namespace CalculatorApi.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var clientIp = GetClientIpAddress(context);
            
            // Логируем начало запроса
            _logger.LogInformation("Incoming request from {ClientIP}: {Method} {Path}", 
                clientIp, context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
                stopwatch.Stop();

                // Логируем успешное завершение
                _logger.LogInformation("Request completed from {ClientIP}: {Method} {Path} - Status: {StatusCode} - Duration: {Elapsed}ms",
                    clientIp, context.Request.Method, context.Request.Path, 
                    context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Логируем ошибку - это попадет в error-логи
                _logger.LogError(ex, "Request failed from {ClientIP}: {Method} {Path} - Duration: {Elapsed}ms",
                    clientIp, context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
                
                // Пробрасываем исключение дальше
                throw;
            }
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            // Проверяем заголовки прокси
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
                return forwardedFor.FirstOrDefault() ?? "Unknown";
            
            if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
                return realIp.FirstOrDefault() ?? "Unknown";
            
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
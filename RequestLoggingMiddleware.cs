// using System.Diagnostics;

// public class RequestLoggingMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<RequestLoggingMiddleware> _logger;

//     public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
//     {
//         _next = next;
//         _logger = logger;
//     }

//     public async Task InvokeAsync(HttpContext context)
//     {
//         // Generate a short correlation id (8 chars from Guid)
//         var correlationId = Guid.NewGuid().ToString("N")[..8];

//         // Set response header before calling next
//         context.Response.Headers["X-Correlation-Id"] = correlationId;

//         var method = context.Request.Method;
//         var path = context.Request.Path;

//         // Start timing
//         var stopwatch = Stopwatch.StartNew();

//         // Log entry
//         _logger.LogInformation("Handling {Method} {Path} (CorrelationId={CorrelationId})",
//             method, path, correlationId);

//         await _next(context); // pass control to next middleware

//         // After next
//         stopwatch.Stop();
//         var statusCode = context.Response.StatusCode;
//         var elapsedMs = stopwatch.ElapsedMilliseconds;

//         // Log exit
//         _logger.LogInformation("Completed {Method} {Path} with {StatusCode} in {Elapsed}ms (CorrelationId={CorrelationId})",
//             method, path, statusCode, elapsedMs, correlationId);
//     }
// }
using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<RequestLoggingMiddleware>
        _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            Guid.NewGuid().ToString("N")[..8];

        context.Response.Headers["X-Correlation-Id"]
            = correlationId;

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Request {Method} {Path} CorrelationId:{CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Response {StatusCode} completed in {ElapsedMs}ms CorrelationId:{CorrelationId}",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}
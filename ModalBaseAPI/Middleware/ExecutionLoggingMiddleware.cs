using System.Diagnostics;

namespace ModelBaseAPI.Middleware
{
    public class ExecutionLoggingMiddleware(RequestDelegate next, ILogger<ExecutionLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExecutionLoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var requestId = Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("RequestId", requestId))
            using (Serilog.Context.LogContext.PushProperty("Ip", ip))
            using (Serilog.Context.LogContext.PushProperty("UserAgent", userAgent))
            {
                _logger.LogInformation("Request {RequestId} started.");

                Exception? exception = null;
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    _logger.LogError(ex, "Error processing {Path}. Exception:{Message}",
                        context.Request.Path, ex.Message);
                    throw;
                }
                finally
                {
                    stopwatch.Stop();

                    _logger.LogInformation("Request {RequestId} finished in {Elapsed}ms with status {StatusCode}",
                        requestId, stopwatch.ElapsedMilliseconds, context.Response?.StatusCode);
                }
            }
        }
    }
    //public class ExecutionLoggingMiddleware(RequestDelegate next, ILogger<ExecutionLoggingMiddleware> logger)
    //{
    //    private readonly RequestDelegate _next = next;
    //    private readonly ILogger<ExecutionLoggingMiddleware> _logger = logger;

    //    public async Task InvokeAsync(HttpContext context)
    //    {
    //        var startTime = DateTime.UtcNow;
    //        _logger.LogInformation("Request started: {StartTime} for {Path}", startTime, context.Request.Path);

    //        var stopwatch = Stopwatch.StartNew();

    //        try
    //        {
    //            await _next(context);
    //        }
    //        catch (Exception ex)
    //        {
    //            stopwatch.Stop();
    //            _logger.LogError(ex, "Unhandled exception for {Path}. Duration: {ElapsedMilliseconds} ms",
    //                context.Request.Path, stopwatch.Elapsed.TotalMilliseconds);
    //            throw;
    //        }

    //        stopwatch.Stop();
    //        var elapsedTime = stopwatch.Elapsed;

    //        if (context.Response.StatusCode >= 400)
    //        {
    //            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
    //            {
    //                _logger.LogWarning("Rate limit exceeded for {Path} at {Time}. Status: {StatusCode}",
    //                    context.Request.Path, elapsedTime.TotalMilliseconds, context.Response.StatusCode);
    //            }
    //            else if (context.Response.StatusCode >= 500)
    //            {
    //                _logger.LogError("Server error at {Path}. Status: {StatusCode}", context.Request.Path, context.Request.Path);
    //            }
    //            else
    //            {
    //                _logger.LogWarning("Client error at {Path}. Status: {StatusCode}", context.Request.Path, context.Request.Path);
    //            }
    //        }

    //        _logger.LogInformation("Request for {Path} completed at {EndTime} after {ElapsedMilliseconds} ms",
    //            context.Request.Path, DateTime.UtcNow, elapsedTime.TotalMilliseconds);
    //    }
    //}
}

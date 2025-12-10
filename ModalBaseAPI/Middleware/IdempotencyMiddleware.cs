using System.Collections.Concurrent;

namespace ModelBaseAPI.Middleware
{
    public class IdempotencyMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private static readonly ConcurrentDictionary<string, object> _store = new();

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var hasIdempotencyAttribute = endpoint?.Metadata.GetMetadata<EnableIdempotencyAttribute>() != null;

            if (!hasIdempotencyAttribute)
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Idempotency-Key header is required.");
                return;
            }

            if (_store.TryGetValue(idempotencyKey!, out var cachedResponse))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync((string)cachedResponse);
                return;
            }

            using var memoryStream = new MemoryStream();
            var originalBodyStream = context.Response.Body;
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);

            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            _store.TryAdd(idempotencyKey!, responseBody);
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class EnableIdempotencyAttribute : Attribute { }
}
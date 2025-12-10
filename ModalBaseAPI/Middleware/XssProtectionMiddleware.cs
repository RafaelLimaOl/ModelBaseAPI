using Ganss.Xss;
using System.Text;

namespace ModelBaseAPI.Middleware
{
    public class XssProtectionMiddleware(RequestDelegate next)
    {

        private readonly RequestDelegate _next = next;

        private static async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                var content = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                return content;
            }
        }

        private static void ValidateHeaders(HttpContext context, HtmlSanitizer sanitizer)
        {
            foreach (var header in context.Request.Headers)
            {
                var sanitised = sanitizer.Sanitize(header.Value);
                if (header.Value != sanitised)
                {
                    throw new Exception($"XSS Detected in Header: {header.Key}");
                }
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sanitizer = new HtmlSanitizer();

            ValidateHeaders(context, sanitizer);

            var content = await ReadRequestBodyAsync(context);
            var sanitised = sanitizer.Sanitize(content);
            if (content != sanitised)
            {
                throw new Exception("XSS Detected in Body.");
            }

            await _next(context);
        }

    }
}

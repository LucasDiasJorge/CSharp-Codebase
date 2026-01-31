using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CustomMiddleware.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private const int MaxBodyLogSize = 4096; // 4KB max body log size

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            // Skip logging for specific paths (e.g., health checks)
            if (ShouldSkipLogging(context))
            {
                await _next(context);
                return;
            }

            // Log the incoming request
            var requestLog = await LogRequest(context);

            // Store the original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                await _next(context);
                stopwatch.Stop();

                // Log the outgoing response
                var responseLog = await LogResponse(context, responseBody, stopwatch.ElapsedMilliseconds);

                // Copy the response to the original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);

                _logger.LogInformation($"Request completed\n{requestLog}\n{responseLog}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private bool ShouldSkipLogging(HttpContext context)
        {
            // Add paths you want to exclude from logging
            var path = context.Request.Path.Value ?? string.Empty;
            return path.StartsWith("/health") || path.StartsWith("/ping");
        }

        private async Task<string> LogRequest(HttpContext context)
        {
            var request = context.Request;
            var sb = new StringBuilder();

            sb.AppendLine("=== Incoming Request ===");
            sb.AppendLine($"[{DateTime.UtcNow:u}] {request.Method} {request.Path}{request.QueryString}");
            sb.AppendLine($"Host: {request.Host}");
            sb.AppendLine($"Content-Type: {request.ContentType}");
            sb.AppendLine($"Content-Length: {request.ContentLength ?? 0}");

            if (request.Body.CanRead && 
                request.ContentLength > 0 && 
                request.ContentLength < MaxBodyLogSize)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(
                    request.Body, 
                    Encoding.UTF8, 
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 1024, 
                    leaveOpen: true);
                
                var body = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    sb.AppendLine("Body:");
                    sb.AppendLine(TruncateIfNeeded(body));
                }
                request.Body.Seek(0, SeekOrigin.Begin);
            }

            return sb.ToString();
        }

        private async Task<string> LogResponse(HttpContext context, MemoryStream responseBody, long durationMs)
        {
            var response = context.Response;
            var sb = new StringBuilder();

            sb.AppendLine("=== Outgoing Response ===");
            sb.AppendLine($"Duration: {durationMs}ms");
            sb.AppendLine($"Status: {response.StatusCode}");
            sb.AppendLine($"Content-Type: {response.ContentType}");
            
            responseBody.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            var bodySize = Encoding.UTF8.GetByteCount(body);
            sb.AppendLine($"Content-Length: {bodySize}");

            if (bodySize > 0 && bodySize < MaxBodyLogSize)
            {
                sb.AppendLine("Body:");
                sb.AppendLine(TruncateIfNeeded(body));
            }
            else if (bodySize >= MaxBodyLogSize)
            {
                sb.AppendLine($"Body: [Truncated - {bodySize} bytes]");
            }

            return sb.ToString();
        }

        private string TruncateIfNeeded(string content)
        {
            if (content.Length > MaxBodyLogSize)
            {
                return content.Substring(0, MaxBodyLogSize) + "...[TRUNCATED]";
            }
            return content;
        }
    }

    // Extension method for easy middleware registration
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
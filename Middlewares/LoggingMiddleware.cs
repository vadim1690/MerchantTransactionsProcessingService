using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionProcessing.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        private const int MaxBodyLength = 4096; // Define a max length for logged bodies

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            Guid traceId = Guid.NewGuid(); // Generate a unique ID for this request/response pair for easier correlation

            // --- Request Logging ---
            string requestBodyContent = await LogRequest(context, traceId);

            // --- Response Logging Setup ---
            var originalResponseBodyStream = context.Response.Body;
            // Use a using statement to ensure the MemoryStream is disposed correctly
            await using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream; // Temporarily replace the response body stream

            string responseBodyContent = string.Empty;
            Exception? caughtException = null;

            try
            {
                // --- Execute Next Middleware ---
                await _next(context);
                stopwatch.Stop(); // Stop timing after the pipeline has executed

                // --- Read Response Body ---
                responseBodyContent = await ReadResponseBody(context.Response, responseBodyStream);

                // --- Log Successful Response ---
                LogResponse(context, traceId, responseBodyContent, stopwatch.ElapsedMilliseconds);

            }
            catch (Exception ex)
            {
                // Stop timing if an exception occurred before it was stopped
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
                caughtException = ex; // Store exception to log later

                // Log minimal response info in case of error before headers are set
                _logger.LogError(ex,
                   "Unhandled exception occurred processing request.\n" +
                   $"TraceID: {traceId}\n" +
                   $"Request Path: {context.Request.Path}\n" +
                   $"Elapsed: {stopwatch.ElapsedMilliseconds}ms");

                // It's often helpful to still log what we know about the response, even if incomplete
                LogResponse(context, traceId, "[Exception occurred - response may be incomplete]", stopwatch.ElapsedMilliseconds, LogLevel.Error);

                // We *don't* read the response body stream here as it might be in an invalid state or empty.
                // Re-throw the exception to be handled by dedicated exception handling middleware or the server default.
                // throw; // Re-throwing here might prevent the finally block from copying the stream back if an Exception Filter handles it. Better to handle copy first.
            }
            finally
            {
                // --- Copy Captured Response Back to Original Stream ---
                // Ensure we only copy if there wasn't a catastrophic failure *before* _next(context) could even run,
                // although the try/catch/finally structure usually ensures this runs.
                // Check if the response has started; if so, copy back. Avoids errors on unhandled exceptions early in the pipeline.
                if (context.Response.HasStarted)
                {
                    _logger.LogDebug($"TraceID: {traceId} - Response has started. Status: {context.Response.StatusCode}. Attempting to copy response stream.");
                    // Seek to the beginning of the memory stream is already done in ReadResponseBody
                    // No need to seek again unless ReadResponseBody failed or was skipped. Ensure it's seeked.
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                }
                else if (caughtException != null)
                {
                    _logger.LogWarning($"TraceID: {traceId} - Response had not started when exception occurred. No response body copied. Status: {context.Response.StatusCode}. Exception: {caughtException.Message}");
                    // If an exception occurred *before* the response started (rare for exceptions *within* _next),
                    // there's nothing to copy. The status code might have been set by exception handling middleware though.
                    // If an error page middleware runs *after* this, it might write to the original stream correctly AFTER this finally block.
                }
                else
                {
                    _logger.LogDebug($"TraceID: {traceId} - Response had not started and no exception caught (e.g., short-circuiting middleware). Status: {context.Response.StatusCode}. No response body copied.");
                    // If middleware before the endpoint short-circuited the request (e.g., authentication failure returning 401)
                    // the response might have been written directly to our memory stream. Copy is still needed.
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                }

                // --- Restore Original Response Stream ---
                context.Response.Body = originalResponseBodyStream; // IMPORTANT: Put the original stream back

                // If an exception was caught, re-throw it *after* stream cleanup
                if (caughtException != null)
                {
                    // Re-throwing allows higher-level exception handlers (like UseExceptionHandler) to work correctly.
                    // System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(caughtException).Throw(); // Preserves stack trace better
                    throw caughtException; // Simpler re-throw
                }
            }
        }

        private async Task<string> LogRequest(HttpContext context, Guid traceId)
        {
            context.Request.EnableBuffering(); // Ensure the request body can be read multiple times

            var request = context.Request;
            string bodyString = "[Not Logged or Empty]"; // Default

            // Only read/log body for specific content types to avoid issues with large files or binary data
            if (request.ContentLength > 0 && IsTextBasedContentType(request.ContentType))
            {
                try
                {
                    // Use a StreamReader to read the body
                    using var reader = new StreamReader(
                        request.Body,
                        encoding: Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: 1024, // Default buffer size
                        leaveOpen: true); // Keep the request stream open for subsequent handlers

                    bodyString = await reader.ReadToEndAsync();

                    // Reset the stream position for the next middleware/handler
                    request.Body.Position = 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"TraceID: {traceId} - Failed to read request body.");
                    bodyString = "[Failed to read request body]";
                    // Attempt to reset position anyway, just in case
                    if (request.Body.CanSeek)
                    {
                        request.Body.Position = 0;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(request.ContentType) && !IsTextBasedContentType(request.ContentType))
            {
                bodyString = $"[Non-Text Content-Type: {request.ContentType}]";
            }


            _logger.LogInformation(
                "Incoming HTTP Request:\n" +
                $"TraceID: {traceId}\n" +
                $"Timestamp: {DateTime.UtcNow:O}\n" + // Use ISO 8601 format
                $"Method: {request.Method}\n" +
                $"Path: {request.Path}\n" +
                $"QueryString: {request.QueryString}\n" +
                $"Schema: {request.Scheme}\n" +
                $"Host: {request.Host}\n" +
                $"RemoteIP: {context.Connection.RemoteIpAddress}\n" +
                $"Headers: {FormatHeaders(request.Headers)}\n" +
                $"Body: {TruncateBody(bodyString)}");

            return bodyString; // Return for potential further use if needed
        }

        private async Task<string> ReadResponseBody(HttpResponse response, MemoryStream responseBodyStream)
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin); // Rewind the stream

            string bodyString = "[Not Logged or Empty]";

            // Check if the response has a body and if it's text-based
            if (responseBodyStream.Length > 0 && IsTextBasedContentType(response.ContentType))
            {
                try
                {
                    // Use a StreamReader to read the captured response body
                    using var reader = new StreamReader(
                        responseBodyStream,
                        encoding: Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: 1024,
                        leaveOpen: true); // Important: leave the stream open so we can copy it later

                    bodyString = await reader.ReadToEndAsync();

                    // No need to reset position here as we read to the end,
                    // but we will seek to beginning before copying in the finally block.
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read response body stream.");
                    bodyString = "[Failed to read response body]";
                }
            }
            else if (!string.IsNullOrEmpty(response.ContentType) && !IsTextBasedContentType(response.ContentType))
            {
                bodyString = $"[Non-Text Content-Type: {response.ContentType}]";
            }
            else if (responseBodyStream.Length == 0)
            {
                bodyString = "[Empty Response Body]";
            }

            // Ensure the stream is ready to be copied back in the finally block
            responseBodyStream.Seek(0, SeekOrigin.Begin);

            return bodyString;
        }

        private void LogResponse(HttpContext context, Guid traceId, string responseBody, long elapsedMs, LogLevel logLevel = LogLevel.Information)
        {
            var response = context.Response;

            _logger.Log(logLevel, // Use specified log level (defaults to Information)
                "Outgoing HTTP Response:\n" +
                $"TraceID: {traceId}\n" +
                $"Timestamp: {DateTime.UtcNow:O}\n" + // Use ISO 8601 format
                $"StatusCode: {response.StatusCode}\n" +
                $"Elapsed: {elapsedMs}ms\n" +
                $"Headers: {FormatHeaders(response.Headers)}\n" +
                $"Body: {TruncateBody(responseBody)}");
        }


        private static bool IsTextBasedContentType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false; // Treat null/empty as non-text

            // Normalize to lower case and check the main part before potential charset/boundary info
            var mediaType = contentType.Split(';')[0].Trim().ToLowerInvariant();

            return mediaType.StartsWith("text/") ||
                   mediaType.EndsWith("json") ||    // application/json, application/problem+json etc.
                   mediaType.EndsWith("xml") ||     // application/xml, application/atom+xml etc.
                   mediaType.EndsWith("javascript") || // application/javascript
                   mediaType.EndsWith("urlencoded"); // application/x-www-form-urlencoded
        }

        private static string FormatHeaders(IHeaderDictionary headers)
        {
            if (headers == null || headers.Count == 0)
                return "[No Headers]";

            // Consider filtering sensitive headers like Authorization, Cookie, Set-Cookie
            var filteredHeaders = headers
                .Where(h => !string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(h.Key, "Cookie", StringComparison.OrdinalIgnoreCase))
                .Select(h =>
                {
                    // Mask Set-Cookie value
                    if (string.Equals(h.Key, "Set-Cookie", StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{h.Key}: [Filtered]";
                    }
                    return $"{h.Key}: {string.Join(", ", h.Value)}";
                });

            return string.Join("\n  ", filteredHeaders); // Newline for better readability
        }

        private static string TruncateBody(string content, int maxLength = MaxBodyLength)
        {
            if (string.IsNullOrEmpty(content))
                return content ?? "[Null Body]"; // Handle null explicitly

            return content.Length <= maxLength
                ? content
                : content.Substring(0, maxLength) + $"... [Truncated - {content.Length - maxLength} more chars]";
        }
    }

    // --- Extension Method for IApplicationBuilder ---
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseHttpLoggingMiddleware(this IApplicationBuilder builder)
        {
            // Consider renaming the extension method to be more specific if you have other logging middlewares
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
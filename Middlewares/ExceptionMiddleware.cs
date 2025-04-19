using MerchantTransactionProcessing.Exceptions;
using MerchantTransactionProcessing.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;
using System.Text.Json;

namespace MerchantTransactionProcessing.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unhandled error occured");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse();
            var message = "";
            switch (ex)
            {
                case ResourceNotFound notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    message = notFoundEx.Message;

                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    message = "An internal server error occurred. Please try again later.";
                    break;
            }

            response.Message = message;
            response.Errors.Add(message);

            var hostEnvironment = context.RequestServices.GetService<IHostEnvironment>();
            if (hostEnvironment != null && !hostEnvironment.IsProduction())
            {
                response.Errors.Add(ex.ToString());
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddlware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}

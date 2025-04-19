using MerchantTransactionProcessing.Models;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MerchantTransactionProcessing.Extensions
{
    public static class ControllerExtensions
    {

        public static ActionResult<ApiResponse<T>> ApiOk<T>(this ControllerBase controller, T data , string? message = null)
        {
            return controller.Ok(ApiResponse<T>.SuccessResponse(data, message));
        }

        public static ActionResult<ApiResponse<T>> ApiCreated<T>(this ControllerBase controller, T data , string? message = null)
        {
            return controller.StatusCode(StatusCodes.Status201Created,ApiResponse.SuccessResponse(data, message ?? "Resource created successfully"));
        }

        public static ActionResult<ApiResponse> ApiNoContent(this ControllerBase controller, string? message = null)
        {
            return controller.StatusCode(StatusCodes.Status204NoContent, ApiResponse.SuccessResponse(message));
        }

        public static ActionResult<ApiResponse> ApiOk(this ControllerBase controller,  string? message = null)
        {
            return controller.Ok(ApiResponse.SuccessResponse( message));
        }

        public static ActionResult<ApiResponse> ApiCreated(this ControllerBase controller,  string? message = null)
        {
            return controller.StatusCode(StatusCodes.Status201Created, ApiResponse.SuccessResponse(message ?? "Resource created successfully"));
        }
    }
}

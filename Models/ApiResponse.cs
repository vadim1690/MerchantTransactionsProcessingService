namespace MerchantTransactionProcessing.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];
        public string ResponseId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse<T>(T? data , string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }
        public static ApiResponse SuccessResponse(string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResponse<T>(string? message = null,List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? []
            };
        }

        public static ApiResponse ErrorResponse(string? message = null, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? []
            };
        }
    }

    public class ApiResponse<T> :ApiResponse
    {
        public T? Data { get; set; }
    }
}

namespace MerchantTransactionProcessing.Models
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public Guid PaymentTransactionId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

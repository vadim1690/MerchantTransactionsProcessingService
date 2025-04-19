namespace MerchantTransactionProcessing.Models.Params
{
    public class RegisterPaymentMethodParams
    {
        public string Method { get; set; } = string.Empty;
        public required Guid MerchantId { get; set; }
    }
}

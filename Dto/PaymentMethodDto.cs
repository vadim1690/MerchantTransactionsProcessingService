namespace MerchantTransactionProcessing.Dto
{
    public class PaymentMethodDto
    {
        public Guid Id { get; set; }
        public string Method {  get; set; } = string.Empty;
    }
}

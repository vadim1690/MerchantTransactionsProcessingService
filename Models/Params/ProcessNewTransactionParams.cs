namespace MerchantTransactionProcessing.Models.Params
{
    public class ProcessNewTransactionParams
    {
        public DateTime? TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public Guid PaymentMethodId { get; set; }
        public Guid MerchantId { get; set; }
    }
}

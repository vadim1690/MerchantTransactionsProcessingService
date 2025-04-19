namespace MerchantTransactionProcessing.Dto
{
    public class ProcessNewTransactionDto
    {
        public DateTime? TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public Guid PaymentMethodId { get; set; }
        public Guid MerchantId { get; set; }
    }
}

namespace MerchantTransactionProcessing.Data.Entities
{
    public class Transaction : BaseEntity
    {
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public required string Status { get; set; }

        public Guid PaymentMethodId { get; set; }
        public virtual PaymentMethod PaymentMethod { get; set; }
        public Guid MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; }
    }
}

namespace MerchantTransactionProcessing.Data.Entities
{
    public class PaymentMethod : BaseEntity
    {
        public string Method { get; set; } = "";
        public string MethodDetails { get; set; } = "";

        public Guid MerchantId { get; set; }
        public virtual Merchant Merchant { get; set; }

    }
}

namespace MerchantTransactionProcessing.Data.Entities
{
    public class Merchant : BaseEntity
    {
        public string Name { get; set; } = "";

        public virtual ICollection<PaymentMethod> PaymentMethods { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}

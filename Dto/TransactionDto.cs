using MerchantTransactionProcessing.Data.Entities;

namespace MerchantTransactionProcessing.Dto
{
    public class TransactionDto
    {
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? PaymentMethodId { get; set; }
        public string? PaymentMethod { get; set; }
        public Guid? MerchantId { get; set; }
        public string? MerchantName { get; set; }
    }
}

using MerchantTransactionProcessing.Data.Entities;

namespace MerchantTransactionProcessing.Dto
{
    public class GetTransactionsDto
    {
        public DateTime? StratDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? Status { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public string? PaymentMethod { get; set; }
        public Guid? MerchantId { get; set; }
        public  string? MerchantName { get; set; }
    }
}

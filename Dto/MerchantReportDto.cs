namespace MerchantTransactionProcessing.Dto
{
    public class MerchantReport
    {
        public Guid MerchantId { get; set; }
        public DateTime ReportDate { get; set; }
        public Summary? Summary { get; set; }
        public List<PaymentMethodStats>? ByPaymentMethod { get; set; }
        public List<HourlyStats> ByHour { get; set; } = [];
    }

    public class Summary
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public decimal AverageTransactionAmount { get; set; }
    }

    public class PaymentMethodStats
    {
        public string PaymentMethodType { get; set; } = "";
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class HourlyStats
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

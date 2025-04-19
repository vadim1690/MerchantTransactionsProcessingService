using MerchantTransactionProcessing.Models;

namespace MerchantTransactionProcessing.Services.PaymentGateway
{
    public interface IPaymentGateway
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
    }
}

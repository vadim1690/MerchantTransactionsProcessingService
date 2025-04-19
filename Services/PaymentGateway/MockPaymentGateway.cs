using MerchantTransactionProcessing.Models;

namespace MerchantTransactionProcessing.Services.PaymentGateway
{
    public class MockPaymentGateway : IPaymentGateway
    {
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            var paymentTransationId = Guid.NewGuid();

            bool isSuccessful = new Random().Next(100) < 90;

            return new PaymentResponse
            {
                Success = isSuccessful,
                PaymentTransactionId = paymentTransationId,
                Message = isSuccessful ? "Payment processed successfully" : "Payment failed"
            };
        }
    }
}

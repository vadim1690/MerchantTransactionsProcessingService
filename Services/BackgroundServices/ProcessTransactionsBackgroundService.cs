

using MerchantTransactionProcessing.Services.PaymentGateway;
using MerchantTransactionProcessing.Services.TransactionService;

namespace MerchantTransactionProcessing.Services.BackgroundServices
{
    public class ProcessTransactionsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ProcessTransactionsBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                await ProcessTransactionsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1),stoppingToken);
            }

        }

        private async Task ProcessTransactionsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var paymentGateway = scope.ServiceProvider.GetRequiredService<IPaymentGateway>();
            var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
            var transactions = await transactionService.GetAllTransactionsForPayment();
            foreach (var transaction in transactions)
            {
                try
                {
                    var result = await paymentGateway.ProcessPaymentAsync(new Models.PaymentRequest { PaymentDetails = transaction.PaymentMethod.MethodDetails });
                    await transactionService.UpdateTransactionStatus(transaction.Id, result.Success ? "Completed" : "Failed");
                }
                catch (Exception ) 
                {
                    await transactionService.UpdateTransactionStatus(transaction.Id,"Failed");
                }
            }
        }
    }
}


using MerchantTransactionProcessing.Data.Entities;
using MerchantTransactionProcessing.Exceptions;
using MerchantTransactionProcessing.Models.Params;
using MerchantTransactionProcessing.Repositories.TransactionRepository;
using MerchantTransactionProcessing.Services.MerchantService;
using System.Linq.Expressions;

namespace MerchantTransactionProcessing.Services.TransactionService
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMerchantService _merchantService;

        public TransactionService(ITransactionRepository transactionRepository, IMerchantService merchantService)
        {
            _transactionRepository = transactionRepository;
            _merchantService = merchantService;
        }

        public async Task<List<Transaction>> GetAllTransactionsForPayment()
        {
            return await _transactionRepository.GetTransactions(transaction => 
                    (transaction.Status == "Pending" || transaction.Status == "Failed") && 
                    (transaction.PaymentMethod.Method == "Credit Card" || transaction.PaymentMethod.Method == "Debit Card"));
        }

        public async Task<Transaction> GetTransactionOrThrow(Guid id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id,t=> t.Merchant,t=>t.PaymentMethod);
            if (transaction == null) 
            {
                throw new ResourceNotFound(id,nameof(Transaction));
            }
            return transaction;
        }

        public async Task<List<Transaction>> GetTransactions(GetTransactionsFiltersParams parameters)
        {
            List<Expression<Func<Transaction, bool>>> filters = [];
            if(parameters.MerchantId.HasValue)
            {
                filters.Add(transaction => transaction.MerchantId == parameters.MerchantId);
            }
            if (!string.IsNullOrEmpty(parameters.MerchantName))
            {
                filters.Add(transaction => transaction.Merchant.Name.Contains(parameters.MerchantName));
            }
            if (parameters.MaxAmount.HasValue)
            {
                filters.Add(transaction => transaction.Amount <= parameters.MaxAmount);
            }
            if (parameters.MinAmount.HasValue)
            {
                filters.Add(transaction => transaction.Amount >= parameters.MinAmount);
            }
            if (parameters.StratDate.HasValue)
            {
                filters.Add(transaction => transaction.TransactionDate >= parameters.StratDate);
            }
            if (parameters.EndDate.HasValue)
            {
                filters.Add(transaction => transaction.TransactionDate <= parameters.EndDate);
            }
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                filters.Add(transaction => transaction.Status == parameters.Status);
            }
            if (parameters.PaymentMethodId.HasValue)
            {
                filters.Add(transaction => transaction.PaymentMethodId == parameters.PaymentMethodId);
            }
            if (!string.IsNullOrEmpty(parameters.PaymentMethod))
            {
                filters.Add(transaction => transaction.PaymentMethod.Method == parameters.PaymentMethod);
            }
            return await _transactionRepository.GetTransactions(filters.ToArray());
        }

        public async Task<Transaction> ProcessNewTransaction(ProcessNewTransactionParams parameters)
        {
            var merchant = await _merchantService.GetMerchantByIdOrThrow(parameters.MerchantId, true);
            var paymentMethod = merchant.PaymentMethods.FirstOrDefault(paymentMethod => paymentMethod.Id == parameters.PaymentMethodId);
            if (paymentMethod == null)
            {
                throw new ResourceNotFound(parameters.PaymentMethodId,nameof(PaymentMethod));
            }
            var transaction = new Transaction
            {
                TransactionDate = parameters.TransactionDate ?? DateTime.UtcNow,
                MerchantId = merchant.Id,
                PaymentMethodId = paymentMethod.Id,
                Amount = parameters.Amount,
                Status = "Pending"
            };
            await _transactionRepository.AddAsync(transaction);
            return transaction;
        }

        public async Task<Transaction> UpdateTransactionStatus(Guid id, string status)
        {
            var transaction = await GetTransactionOrThrow(id);
            transaction.Status = status;
            await _transactionRepository.UpdateAsync(transaction);
            return transaction;
        }
    }
}

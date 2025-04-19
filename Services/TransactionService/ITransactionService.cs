using MerchantTransactionProcessing.Data.Entities;
using MerchantTransactionProcessing.Models.Params;

namespace MerchantTransactionProcessing.Services.TransactionService
{
    public interface ITransactionService
    {
        Task<Transaction> ProcessNewTransaction(ProcessNewTransactionParams parameters);
        Task<Transaction> GetTransactionOrThrow(Guid id);
        Task<Transaction> UpdateTransactionStatus(Guid id,string status);
        Task<List<Transaction>> GetTransactions(GetTransactionsFiltersParams parameters);
        Task<List<Transaction>> GetAllTransactionsForPayment();
    }
}


using MerchantTransactionProcessing.Data.Entities;
using System.Linq.Expressions;

namespace MerchantTransactionProcessing.Repositories.TransactionRepository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<List<Transaction>> GetTransactions(params Expression<Func<Transaction, bool>>[] filters);

    }
}

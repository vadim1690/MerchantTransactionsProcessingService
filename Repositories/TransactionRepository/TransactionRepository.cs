using MerchantTransactionProcessing.Data;
using MerchantTransactionProcessing.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace MerchantTransactionProcessing.Repositories.TransactionRepository
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Transaction>> GetTransactions(params Expression<Func<Transaction, bool>>[] filters)
        {
            var query = _dbContext.Transactions.AsQueryable();
            query = query.Include(t => t.Merchant).Include(t => t.PaymentMethod);
            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    query = query.Where(filter);
                }
            }
            return await query.ToListAsync();
        }
    }
}

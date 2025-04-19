using MerchantTransactionProcessing.Data;
using MerchantTransactionProcessing.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MerchantTransactionProcessing.Repositories.MerchantRepository
{
    public class MerchantRepository : Repository<Merchant>, IMerchantRepository
    {
        public MerchantRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}

using MerchantTransactionProcessing.Data;
using MerchantTransactionProcessing.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MerchantTransactionProcessing.Repositories.PaymentMethodRepository
{
    public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
    {
        public PaymentMethodRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
    }
}

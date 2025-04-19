using MerchantTransactionProcessing.Data.Entities;
using MerchantTransactionProcessing.Dto;
using MerchantTransactionProcessing.Models.Params;

namespace MerchantTransactionProcessing.Services.MerchantService
{
    public interface IMerchantService
    {
        Task<PaymentMethod> RegisterPaymentMethod(RegisterPaymentMethodParams parameters);
        Task<List<PaymentMethod>> GetPaymentMethods(Guid merchantId); 
        Task<List<Transaction>> GetMerchantTransactions(Guid merchantId);
        Task<MerchantReport> GetMerchantDailyReoprt(Guid merchantId,DateTime? date);
        Task<List<Merchant>> GetAllMerchants(); 
        Task<Merchant?> GetMerchantById(Guid id,bool includePaymentMethods = false,bool includeTransactions = false);
        Task<Merchant> GetMerchantByIdOrThrow(Guid id, bool includePaymentMethods = false, bool includeTransactions = false);
    }
}
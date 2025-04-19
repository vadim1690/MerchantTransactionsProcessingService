using MerchantTransactionProcessing.Constants;
using MerchantTransactionProcessing.Data.Entities;
using MerchantTransactionProcessing.Dto;
using MerchantTransactionProcessing.Exceptions;
using MerchantTransactionProcessing.Models.Params;
using MerchantTransactionProcessing.Repositories.MerchantRepository;
using MerchantTransactionProcessing.Repositories.PaymentMethodRepository;
using MerchantTransactionProcessing.Repositories.TransactionRepository;
using MerchantTransactionProcessing.Services.CacheService;
using System.Linq.Expressions;


namespace MerchantTransactionProcessing.Services.MerchantService
{
    public class MerchantService : IMerchantService
    {
        private readonly IMerchantRepository _merchantRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICacheService _cache;

        public MerchantService(IMerchantRepository merchantRepository, IPaymentMethodRepository paymentMethodRepository, ITransactionRepository transactionRepository, ICacheService cache)
        {
            _merchantRepository = merchantRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _transactionRepository = transactionRepository;
            _cache = cache;
        }

        public async Task<List<Merchant>> GetAllMerchants()
        {
            var cachedData = await _cache.GetAsync<List<Merchant>>(CacheConstants.ALL_MERCHANTS_KEY);
            if (cachedData != null)
            {
                return cachedData;
            }
            var resposne = (List<Merchant>)await _merchantRepository.ListAllAsync();
            await Task.Delay(TimeSpan.FromSeconds(5)); // delay for caching benefits to be visible

            await _cache.SetAsync(CacheConstants.ALL_MERCHANTS_KEY,resposne, TimeSpan.FromMinutes(5));
            return resposne;
        }

        public async Task<Merchant?> GetMerchantById(Guid id, bool includePaymentMethods = false, bool includeTransactions = false)
        {
            var cachedData = await _cache.GetAsync<Merchant>($"{CacheConstants.MERCHANT_KEY}_{id}");
            if (cachedData != null)
            {
                return cachedData;
            }
            List<Expression<Func<Merchant, object>>> includes = [];
            if(includePaymentMethods)
            {
                includes.Add(merchant => merchant.PaymentMethods);
            }
            if (includeTransactions)
            {
                includes.Add(merchant => merchant.Transactions);
            }
            var merchant = await _merchantRepository.GetByIdAsync(id, includes.ToArray());
            if(merchant != null)
            {
                await _cache.SetAsync($"{CacheConstants.MERCHANT_KEY}_{id}", merchant, TimeSpan.FromMinutes(5));
            }
            return merchant;
        }

        public async Task<Merchant> GetMerchantByIdOrThrow(Guid id, bool includePaymentMethods = false, bool includeTransactions = false)
        {
            var cachedData = await _cache.GetAsync<Merchant>($"{CacheConstants.MERCHANT_KEY}_{id}");
            if (cachedData != null)
            {
                return cachedData;
            }
            var merchant = await GetMerchantById(id,includePaymentMethods,includeTransactions);
            if (merchant == null)
            {
                throw new ResourceNotFound(id,nameof(Merchant));
            }
            await _cache.SetAsync($"{CacheConstants.MERCHANT_KEY}_{id}", merchant, TimeSpan.FromMinutes(5));
            return merchant;
        }

        public async Task<MerchantReport> GetMerchantDailyReoprt(Guid merchantId, DateTime? date)
        {
            var cachedData = await _cache.GetAsync<MerchantReport>($"{CacheConstants.MERCHANT_KEY}_{merchantId}_{date}");
            if (cachedData != null)
            {
                return cachedData;
            }
            var merchant = await GetMerchantByIdOrThrow(merchantId);
            var compareDate = date ?? DateTime.UtcNow.Date;
            var transactions = await _transactionRepository.GetTransactions(
                    t => t.MerchantId == merchant.Id,
                    t => t.TransactionDate >= compareDate,
                    t => t.TransactionDate < compareDate.AddDays(1)
            );
            var report = new MerchantReport
            {
                MerchantId = merchantId,
                ReportDate = compareDate,
                Summary = transactions.GroupBy(g => 1).Select(group => new Summary
                {
                    TotalTransactions = group.Count(),
                    TotalAmount = group.Sum(t => t.Amount),
                    SuccessfulTransactions = group.Count(t => t.Status != "Failed"),
                    FailedTransactions = group.Count(t => t.Status == "Failed"),
                    AverageTransactionAmount = group.Average(t => t.Amount)

                }).FirstOrDefault(),

                ByPaymentMethod = transactions.GroupBy(g => g.PaymentMethod.Method).Select(group => new PaymentMethodStats
                {
                    PaymentMethodType = group.Key,
                    Count = group.Count(),
                    TotalAmount = group.Sum(t => t.Amount),
                }).ToList(),

                ByHour = transactions.GroupBy(g => g.TransactionDate.Hour).Select(group => new HourlyStats
                {
                    Hour = group.Key,
                    Count = group.Count(),
                    TotalAmount = group.Sum(t => t.Amount),
                }).ToList(),
            };
            await _cache.SetAsync($"{CacheConstants.MERCHANT_KEY}_{merchantId}_{date}", report, TimeSpan.FromMinutes(5));
            return report;

        }

        public async Task<List<Transaction>> GetMerchantTransactions(Guid merchantId)
        {
            var merchant = await GetMerchantByIdOrThrow(merchantId);
            return await _transactionRepository.GetTransactions(t => t.MerchantId == merchant.Id);
        }

        public async Task<List<PaymentMethod>> GetPaymentMethods(Guid merchantId)
        {
            var merchant = await GetMerchantByIdOrThrow(merchantId);
            return (List<PaymentMethod>)await _paymentMethodRepository.FilterAsync(paymentMethod => paymentMethod.MerchantId == merchantId);
        }

        public async Task<PaymentMethod> RegisterPaymentMethod(RegisterPaymentMethodParams parameters)
        {
            var merchant = await GetMerchantByIdOrThrow(parameters.MerchantId);
            var paymentMethod = new PaymentMethod
            {
                Method = parameters.Method,
                MerchantId = merchant.Id,
            };
            await _paymentMethodRepository.AddAsync(paymentMethod);
            return paymentMethod;

        }
    }
}

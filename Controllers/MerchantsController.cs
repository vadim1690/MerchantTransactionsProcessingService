using MerchantTransactionProcessing.Dto;
using MerchantTransactionProcessing.Extensions;
using MerchantTransactionProcessing.Models;
using MerchantTransactionProcessing.Models.Params;
using MerchantTransactionProcessing.Services.MerchantService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MerchantTransactionProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        private readonly IMerchantService _merchantService;

        public MerchantsController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }
        [HttpPost("{merchantId}/payment-methods")]
        public async Task<ActionResult<ApiResponse<PaymentMethodDto>>> RegisterPaymentMethod(Guid merchantId, [FromBody] RegisterPaymentMethodDto dto)
        {
            var paymentMethod = await _merchantService.RegisterPaymentMethod(new RegisterPaymentMethodParams { MerchantId = merchantId, Method = dto.Method });
            var paymentMethodDto = new PaymentMethodDto { Id = paymentMethod.Id, Method = paymentMethod.Method };
            return this.ApiCreated(paymentMethodDto);
        }

        [HttpGet("{merchantId}/payment-methods")]
        public async Task<ActionResult<ApiResponse<List<PaymentMethodDto>>>> GetPaymentMethods(Guid merchantId)
        {
            var paymentMethods = await _merchantService.GetPaymentMethods(merchantId);
            var paymentMethodsDto = paymentMethods.Select(paymentMethod => new PaymentMethodDto { Id = paymentMethod.Id, Method = paymentMethod.Method }).ToList();
            return this.ApiOk(paymentMethodsDto);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MerchantDto>>>> GetMerchants()
        {
            var merchants = await _merchantService.GetAllMerchants();
            return this.ApiOk(merchants.Select(merchant => new MerchantDto { Id = merchant.Id,Name = merchant.Name}).ToList());
        }

        [HttpGet("{merchantId}/transactions")]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetMerchantTransactions(Guid merchantId)
        {
            var transactions = await _merchantService.GetMerchantTransactions(merchantId);
            return this.ApiOk(transactions.Select(transaction => new TransactionDto
            {
                Amount = transaction.Amount,
                Status = transaction.Status,
                MerchantId = transaction.MerchantId,
                PaymentMethodId = transaction.PaymentMethodId,
                TransactionDate = transaction.TransactionDate,
                MerchantName = transaction.Merchant.Name,
                PaymentMethod = transaction.PaymentMethod.Method
            }).ToList());
        }

        [HttpGet("{merchantId}/reports/daily")]
        public async Task<ActionResult<ApiResponse<MerchantReport>>> GetMerchantDailyReport(Guid merchantId, [FromQuery]DateTime? date)
        {
            var report = await _merchantService.GetMerchantDailyReoprt(merchantId,date);
            return this.ApiOk(report);
        }
    }
}

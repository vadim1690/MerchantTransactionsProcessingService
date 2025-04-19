using MerchantTransactionProcessing.Dto;
using MerchantTransactionProcessing.Extensions;
using MerchantTransactionProcessing.Models;
using MerchantTransactionProcessing.Models.Params;
using MerchantTransactionProcessing.Services.TransactionService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MerchantTransactionProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        [HttpPost]
        public async Task<ActionResult<ApiResponse<NewTransactionCreatedDto>>> ProcessNewTransaction(ProcessNewTransactionDto dto)
        {
            var transaction = await _transactionService.ProcessNewTransaction(new ProcessNewTransactionParams
            {
                TransactionDate = dto.TransactionDate,
                Amount = dto.Amount,
                PaymentMethodId = dto.PaymentMethodId,
                MerchantId = dto.MerchantId,
            });
            return this.ApiCreated(new NewTransactionCreatedDto { TransactionId = transaction.Id });
        }

        [HttpGet("{transactionId}")]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction(Guid transactionId)
        {
            var transaction = await _transactionService.GetTransactionOrThrow(transactionId);
            return this.ApiCreated(new TransactionDto { 
                Amount = transaction.Amount,
                MerchantId = transaction.MerchantId,
                PaymentMethodId = transaction.PaymentMethodId,
                TransactionDate = transaction.TransactionDate,
                MerchantName = transaction.Merchant.Name,
                PaymentMethod = transaction.PaymentMethod.Method
            });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetTransactions([FromQuery]GetTransactionsDto dto)
        {
            var transactions = await _transactionService.GetTransactions(new GetTransactionsFiltersParams
            {
                StratDate = dto.StratDate,
                EndDate = dto.EndDate,
                MaxAmount = dto.MaxAmount,
                MinAmount = dto.MinAmount,
                MerchantId = dto.MerchantId,
                MerchantName = dto.MerchantName,
                PaymentMethod = dto.PaymentMethod,
                PaymentMethodId = dto.PaymentMethodId,
                Status = dto.Status
            });
            return this.ApiOk(transactions.Select(transaction=> new TransactionDto
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
    }
}

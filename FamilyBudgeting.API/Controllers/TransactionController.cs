using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FamilyBudgeting.Application.DTOs.Requests.Transactions;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class TransactionController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionRequest request)
        {
            Guid userId = GetUserIdFromToken();

            return (await _transactionService.CreateTransactionAsync(userId, request)).ToActionResult();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTransaction(UpdateTransactionRequest request)
        {
            Guid userId = GetUserIdFromToken();
            return (await _transactionService.UpdateTransactionAsync(userId, request)).ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTransaction([FromQuery] DeleteTransactionRequest request)
        {
            Guid userId = GetUserIdFromToken();
            return (await _transactionService.DeleteTransactionAsync(userId, request)).ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] GetUserBudgetTransactionsRequest request)
        {
            Guid userId = GetUserIdFromToken();

            var transRequest = new GetTransactionsFromLedgerRequest(
                userId,
                request.LedgerId,
                request.StartDate,
                request.EndDate,
                request.BudgetId,
                request.CategoryId,
                request.BudgetCategoryId,
                request.Page,
                request.PageSize
            );

            return (await _transactionService.GetTransactionsFromLedgerAsync(transRequest)).ToActionResult();
        }

        [HttpGet("create-transaction-page-data")]
        public async Task<IActionResult> GetCreateTransactionPageData([FromQuery] GetCreateTransactionPageDataRequest request)
        {
            Guid userId = GetUserIdFromToken();

            return (await _transactionService.GetCreateTransactionPageDataAsync(userId, request.budgetId, request.ledgerId)).ToActionResult();
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferTransactionRequest request)
        {
            Guid userId = GetUserIdFromToken();
            return (await _transactionService.TransferAsync(userId, request)).ToActionResult();
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportTransactions([FromForm] TransactionImportRequest request)
        {
            Guid userId = GetUserIdFromToken();
            return (await _transactionService.ImportTransactionsAsync(userId, request.LedgerId, request.File)).ToActionResult();
        }
    }
}

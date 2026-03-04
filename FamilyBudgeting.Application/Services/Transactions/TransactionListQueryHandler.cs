using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionListQueryHandler : ITransactionListQueryHandler
    {
        private readonly ITransactionQueryService _transactionQueryService;
        private readonly ITransactionAccessPolicy _transactionAccessPolicy;

        public TransactionListQueryHandler(
            ITransactionQueryService transactionQueryService,
            ITransactionAccessPolicy transactionAccessPolicy)
        {
            _transactionQueryService = transactionQueryService;
            _transactionAccessPolicy = transactionAccessPolicy;
        }

        public async Task<Result<PaginatedTransactionListResponse>> HandleAsync(GetTransactionsFromLedgerRequest request)
        {
            if (request.LedgerId.HasValue)
            {
                var accessResult = await _transactionAccessPolicy.EnsureLedgerAccessAsync(request.UserId, request.LedgerId.Value);
                if (!accessResult.IsSuccess)
                {
                    return Result.Forbidden(accessResult.Errors.FirstOrDefault() ?? "User does not have access to this ledger");
                }
            }

            var (items, totalCount) = await _transactionQueryService.GetTransactionListPaginatedAsync(
                request.LedgerId,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue),
                request.BudgetId,
                request.CategoryId,
                request.BudgetCategoryId,
                request.UserId,
                request.Page,
                request.PageSize);

            if (items is null)
            {
                return Result.NotFound("Transactions not found");
            }

            var summary = await _transactionQueryService.GetTransactionSummaryAsync(
                request.LedgerId,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue),
                request.BudgetId,
                request.CategoryId,
                request.BudgetCategoryId,
                request.UserId);

            return Result.Success(new PaginatedTransactionListResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                Summary = summary
            });
        }
    }
}

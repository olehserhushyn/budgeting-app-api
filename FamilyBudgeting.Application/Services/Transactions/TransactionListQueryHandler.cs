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
        private readonly IUserLedgerQueryService _userLedgerQueryService;

        public TransactionListQueryHandler(
            ITransactionQueryService transactionQueryService,
            IUserLedgerQueryService userLedgerQueryService)
        {
            _transactionQueryService = transactionQueryService;
            _userLedgerQueryService = userLedgerQueryService;
        }

        public async Task<Result<PaginatedTransactionListResponse>> HandleAsync(GetTransactionsFromLedgerRequest request)
        {
            if (request.LedgerId.HasValue)
            {
                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(request.UserId, request.LedgerId.Value);
                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
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

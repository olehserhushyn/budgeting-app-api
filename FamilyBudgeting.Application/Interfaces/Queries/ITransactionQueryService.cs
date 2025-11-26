using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Transactions;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface ITransactionQueryService
    {
        Task<IEnumerable<TransactionDto>> GetTransactionsFromLedgerAsync(Guid? ledgerId,
            DateTime startDate, DateTime endDate, Guid? BudgetId);
        Task<IEnumerable<GetTransactionListResponse>> GetTransactionListAsync(
            Guid? ledgerId,
            DateTime startDate,
            DateTime endDate,
            Guid? budgetId,
            Guid? categoryId,
            Guid? budgetCategoryId,
            Guid? userId = null
        );
        Task<(IEnumerable<GetTransactionListResponse> Items, int TotalCount)> GetTransactionListPaginatedAsync(
            Guid? ledgerId,
            DateTime startDate,
            DateTime endDate,
            Guid? budgetId,
            Guid? categoryId,
            Guid? budgetCategoryId,
            Guid? userId,
            int page,
            int pageSize
        );
        IQueryBuilder<TransactionDto?> GetTransactionById(Guid transactionId);
        Task<LedgerTransactionStatisticsResponse> GetLedgerTransactionStatisticsAsync(GetLedgerTransactionStatisticsRequest request);
        Task<GetTransactionListResponse_Summary> GetTransactionSummaryAsync(
            Guid? ledgerId,
            DateTime startDate,
            DateTime endDate,
            Guid? budgetId,
            Guid? categoryId,
            Guid? budgetCategoryId,
            Guid? userId);
    }
}

using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionListQueryHandler
    {
        Task<Result<PaginatedTransactionListResponse>> HandleAsync(GetTransactionsFromLedgerRequest request);
    }
}

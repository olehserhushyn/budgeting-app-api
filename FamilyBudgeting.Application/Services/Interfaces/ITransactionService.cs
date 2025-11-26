using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<Result<Guid>> CreateTransactionAsync(Guid userId, CreateTransactionRequest request);
        Task<Result<bool>> UpdateTransactionAsync(Guid userId, UpdateTransactionRequest request);
        Task<Result<bool>> DeleteTransactionAsync(Guid userId, DeleteTransactionRequest request);
        Task<Result<PaginatedTransactionListResponse>> GetTransactionsFromLedgerAsync(GetTransactionsFromLedgerRequest request);
        Task<Result<GetCreateTransactionPageDataResponse>> GetCreateTransactionPageDataAsync(Guid userId, Guid? budgetId, Guid? ledgerId);
        Task<Result<Guid>> TransferAsync(Guid userId, TransferTransactionRequest request);
        Task<Result<int>> ImportTransactionsAsync(Guid userId, Guid ledgerId, Microsoft.AspNetCore.Http.IFormFile file);
    }
}

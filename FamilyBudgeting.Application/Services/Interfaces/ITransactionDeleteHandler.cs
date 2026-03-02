using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionDeleteHandler
    {
        Task<Result<bool>> HandleAsync(Guid userId, DeleteTransactionRequest request);
    }
}

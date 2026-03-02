using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionUpdateHandler
    {
        Task<Result<bool>> HandleAsync(Guid userId, UpdateTransactionRequest request);
    }
}

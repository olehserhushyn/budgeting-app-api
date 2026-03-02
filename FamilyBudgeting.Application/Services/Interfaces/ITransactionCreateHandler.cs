using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionCreateHandler
    {
        Task<Result<Guid>> HandleAsync(Guid userId, CreateTransactionRequest request);
    }
}

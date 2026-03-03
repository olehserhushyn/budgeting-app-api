using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionCategoryResolutionPolicy
    {
        Task<Result<(Guid? CategoryId, CreateTransactionRequest Request)>> ResolveForCreateAsync(
            Guid userId,
            Guid ledgerId,
            CreateTransactionRequest request);
    }
}

using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionPageDataQueryHandler
    {
        Task<Result<GetCreateTransactionPageDataResponse>> HandleAsync(Guid userId, Guid? budgetId, Guid? ledgerId);
    }
}

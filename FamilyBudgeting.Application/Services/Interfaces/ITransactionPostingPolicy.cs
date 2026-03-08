using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Accounts;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionPostingPolicy
    {
        Task<Result> ApplyBudgetImpactForCreateAsync(Guid ledgerId, CreateTransactionRequest request, int centsAmountWithSign);
        Task<Result> ApplyAccountImpactForCreateAsync(AccountCurrencyDetailsDto accountDto, int centsAmountWithSign);
    }
}

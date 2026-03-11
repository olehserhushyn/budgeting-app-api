using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Accounts;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionPostingPolicy
    {
        Task<Result> ApplyBudgetImpactForCreateAsync(Guid ledgerId, CreateTransactionRequest request, int centsAmountWithSign);
        Task<Result> ApplyAccountImpactForCreateAsync(AccountCurrencyDetailsDto accountDto, int centsAmountWithSign);

        Task<Result> ApplyBudgetImpactForUpdateAsync(Guid ledgerId, UpdateTransactionRequest request, int newCentsAmountWithSign);
        Task<Result> ApplyAccountImpactForUpdateAsync(AccountCurrencyDetailsDto accountDto, int existingCentsAmountWithSign, int newCentsAmountWithSign);

        Task<Result> ApplyAccountImpactForDeleteAsync(AccountCurrencyDetailsDto accountDto, int existingCentsAmountWithSign);
    }
}

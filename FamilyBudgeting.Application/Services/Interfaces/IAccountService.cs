using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Accounts;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IAccountService
    {
        Task<Result<Guid>> CreateAccountAsync(Guid userId, CreateAccountRequest request);
        Task<Result<IEnumerable<DTOs.Models.Accounts.AccountCurrencyDetailsDto>>> GetAccountsAsync(Guid userId);
        Task<Result<DTOs.Models.Accounts.AccountCurrencyDetailsDto>> GetAccountAsync(Guid userId, Guid accountId);
        Task<Result> UpdateAccountAsync(Guid userId, UpdateAccountRequest request);
        Task<Result> DeleteAccountAsync(Guid userId, Guid accountId);
    }
}

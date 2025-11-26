using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Accounts;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IAccountQueryService
    {
        Task<IEnumerable<DTOs.Models.Accounts.AccountCurrencyDetailsDto>> GetAccountsAsync(Guid userId);
        Task<DTOs.Models.Accounts.AccountCurrencyDetailsDto?> GetAccountAsync(Guid accountId);
        Task<AccountDto?> GetAccountDtoAsync(Guid accountId);
        IQueryBuilder<AccountCurrencyDetailsDto?> GetAccountCurrencyDetailsAsync(Guid accountId);
    }
}

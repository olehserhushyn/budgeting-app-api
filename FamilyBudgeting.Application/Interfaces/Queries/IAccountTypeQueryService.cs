using FamilyBudgeting.Domain.DTOs.Models.AccountTypes;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IAccountTypeQueryService
    {
        Task<AccountTypeDto?> GetAccountTypeEntity(Guid accountTypeId);
        Task<IEnumerable<AccountTypeDto>> GetAccountTypes();
    }
}

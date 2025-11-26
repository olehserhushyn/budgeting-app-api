using FamilyBudgeting.Domain.DTOs.Models.UserBudgets;
using FamilyBudgeting.Domain.DTOs.Models.UserLedgers;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IUserBudgetQueryService
    {
        Task<bool> IsUserInLedgerRoleAsync(Guid userId, Guid budgetId, params string[] roleTitles);
        Task<UserBudgetDto?> GetUserBudgetDtoByIdAsync(Guid budgetId, Guid id);
        Task<UserBudgetDto?> GetUserBudgetDtoByUserIdAsync(Guid budgetId, Guid userId);
        Task<IEnumerable<UserBudgetDetailsDto>> GetUserBudgetDtoByBudgetIdAsync(Guid budgetId);
        Task<IEnumerable<UserBudgetDetailsDto>> GetUserLedgerDtoByBudgetIdAsync(Guid budgetId);
    }
}

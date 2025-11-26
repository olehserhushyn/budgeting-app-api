using FamilyBudgeting.Domain.DTOs.Models.Budgets;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IBudgetQueryService
    {
        Task<IEnumerable<BudgetDto>> GetBudgetsFromLedgerAsync(Guid ledgerId);
        Task<Guid> GetLedgerIdFromBudgetAsync(Guid budgetId);
        Task<BudgetDto?> GetBudgetAsync(Guid budgetId);
    }
}

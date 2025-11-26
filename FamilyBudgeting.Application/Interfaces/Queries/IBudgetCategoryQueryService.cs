using FamilyBudgeting.Domain.DTOs.Models.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IBudgetCategoryQueryService
    {
        Task<IEnumerable<GetBudgetCategoriesDetailedResponse>> GetBudgetCategoriesAsync(Guid ledgerId, Guid budgetId);
        Task<GetBudgetCategoriesDetailedResponse?> GetBudgetCategoryAsync(Guid ledgerId, Guid budgetId, Guid id);
        Task<IEnumerable<BudgetCategoriesDetailedResponse>> GetBudgetCategoriesDetailedAsync(Guid ledgerId, Guid budgetId);
        Task<BudgetCategoryDto?> GetBudgetCategoryAsync(Guid id);
    }
}

namespace FamilyBudgeting.Domain.Data.BudgetCategories
{
    public interface IBudgetCategoryRepository
    {
        Task<Guid> CreateBudgetCategoryAsync(BudgetCategory budgetCategory);
        Task<bool> UpdateBudgetCategoryAsync(Guid id, BudgetCategory budgetCategory);
    }
}

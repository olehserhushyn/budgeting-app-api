namespace FamilyBudgeting.Domain.Data.Budgets
{
    public interface IBudgetRepository
    {
        Task<Guid> CreateBudgetAsync(Budget budget);
        Task<bool> UpdateBudgetAsync(Guid id, Budget budget);
    }
}

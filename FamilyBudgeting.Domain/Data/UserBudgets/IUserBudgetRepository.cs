namespace FamilyBudgeting.Domain.Data.UserBudgets
{
    public interface IUserBudgetRepository
    {
        Task<Guid> CreateUserBudgetAsync(UserBudget userBudget);
        Task<bool> UpdateUserBudgetAsync(Guid id, UserBudget userBudget);
    }
} 
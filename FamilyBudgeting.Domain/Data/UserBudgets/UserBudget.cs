namespace FamilyBudgeting.Domain.Data.UserBudgets
{
    public class UserBudget : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public Guid BudgetId { get; private set; }

        public UserBudget(Guid userId, Guid roleId, Guid budgetId)
        {
            UserId = userId;
            RoleId = roleId;
            BudgetId = budgetId;
        }

        public void Update(Guid roleId)
        {
            this.RoleId = roleId;

            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}

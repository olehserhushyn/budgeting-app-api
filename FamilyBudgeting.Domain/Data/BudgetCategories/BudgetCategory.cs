namespace FamilyBudgeting.Domain.Data.BudgetCategories
{
    public class BudgetCategory : BaseEntity
    {
        public Guid BudgetId { get; private set; }
        public Guid CategoryId { get; private set; }
        public Guid CurrencyId { get; private set; }
        public int PlannedAmount { get; private set; }
        public int CurrentAmount { get; private set; }

        public BudgetCategory(Guid budgetId, Guid categoryId, Guid currencyId, int plannedAmount, int currentAmount)
        {
            BudgetId = budgetId;
            CategoryId = categoryId;
            CurrencyId = currencyId;
            PlannedAmount = plannedAmount;
            CurrentAmount = currentAmount;
        }

        public void AddTransaction(int amount)
        {
            CurrentAmount += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(Guid categoryId, int plannedAmount)
        {
            int tempSpent = PlannedAmount - CurrentAmount;
            CategoryId = categoryId;
            PlannedAmount = plannedAmount;
            CurrentAmount = plannedAmount - tempSpent;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            this.IsDeleted = true;
            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}

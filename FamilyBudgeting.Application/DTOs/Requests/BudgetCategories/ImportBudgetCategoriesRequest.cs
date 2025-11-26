namespace FamilyBudgeting.Domain.DTOs.Requests.BudgetCategories
{
    public enum ImportBudgetCategoriesMode
    {
        DefaultOnly = 0, // Only categories where plannedAmount == currentAmount
        CarryOver = 1    // All categories, add (planned - current) to planned
    }

    public class ImportBudgetCategoriesRequest
    {
        public Guid SourceBudgetId { get; set; }
        public Guid TargetBudgetId { get; set; }
        public ImportBudgetCategoriesMode Mode { get; set; }
    }
} 
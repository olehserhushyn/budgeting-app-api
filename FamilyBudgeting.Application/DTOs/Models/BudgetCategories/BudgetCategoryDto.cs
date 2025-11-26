namespace FamilyBudgeting.Domain.DTOs.Models.BudgetCategories
{
    public class BudgetCategoryDto
    {
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid BudgetId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid CurrencyId { get; set; }
        public int PlannedAmount { get; set; }
        public int CurrentAmount { get; set; }
        public int CurrencyFractionalUnitFactor { get; set; }
    }
}

namespace FamilyBudgeting.Domain.DTOs.Requests.BudgetCategories
{
    public record UpdateBudgetCategoryRequest(Guid CategoryId, double PlannedAmount);
}

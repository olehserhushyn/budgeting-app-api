namespace FamilyBudgeting.Domain.DTOs.Requests.Categories
{
    public record CreateBudgetCategoryRequest(string Title, Guid BudgetId, 
        Guid CurrencyId, double PlannedAmount, int initialPlannedAmount, Guid TransactionTypeId);
}

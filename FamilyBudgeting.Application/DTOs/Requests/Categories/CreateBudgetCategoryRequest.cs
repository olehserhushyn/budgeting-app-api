namespace FamilyBudgeting.Domain.DTOs.Requests.Categories
{
    public record CreateBudgetCategoryRequest(string Title, Guid BudgetId, 
        Guid CurrencyId, double PlannedAmount, Guid TransactionTypeId);
}

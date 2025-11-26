namespace FamilyBudgeting.Domain.DTOs.Requests.Categories
{
    public record UpdateCategoryRequest(Guid Id, string Title, Guid BudgetId, int PlannedAmount, Guid LedgerId, Guid TransactionTypeId);
}

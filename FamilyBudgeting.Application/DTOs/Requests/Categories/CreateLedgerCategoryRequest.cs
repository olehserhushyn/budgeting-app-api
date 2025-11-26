namespace FamilyBudgeting.Domain.DTOs.Requests.Categories
{
    public record CreateLedgerCategoryRequest(Guid LedgerId, string Title, Guid TransactionTypeId);
}

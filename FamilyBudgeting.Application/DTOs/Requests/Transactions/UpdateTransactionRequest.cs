namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record UpdateTransactionRequest(
        Guid TransactionId,
        Guid AccountId,
        Guid? LedgerId,
        Guid TransactionTypeId,
        Guid? CategoryId,
        Guid? BudgetId,
        double Amount,
        DateTime Date,
        string? Note,
        Guid? BudgetCategoryId
    );
}

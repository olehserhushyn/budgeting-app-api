namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record CreateTransactionRequest(
        Guid AccountId,
        Guid? LedgerId,
        Guid TransactionTypeId,
        Guid? CategoryId,
        Guid? BudgetId,
        double Amount,
        DateTime Date,
        string? Note,
        Guid? BudgetCategoryId,
        string? CategoryTitle,
        string? BudgetCategoryTitle
    );
}

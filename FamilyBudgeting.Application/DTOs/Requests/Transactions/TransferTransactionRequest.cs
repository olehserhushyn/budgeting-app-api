namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record TransferTransactionRequest(
        Guid SourceAccountId,
        Guid DestinationAccountId,
        Guid? LedgerId,
        double Amount,
        DateTime Date,
        string? Note,
        Guid? BudgetId,
        Guid? BudgetCategoryId
    );
} 
namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record GetTransactionsFromLedgerRequest(
        Guid UserId,
        Guid? LedgerId,
        DateOnly StartDate,
        DateOnly EndDate,
        Guid? BudgetId,
        Guid? CategoryId,
        Guid? BudgetCategoryId,
        int Page = 0,
        int PageSize = 0
    );
}

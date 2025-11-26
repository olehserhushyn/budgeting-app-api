namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record GetUserBudgetTransactionsRequest(
        Guid? LedgerId,
        DateOnly StartDate,
        DateOnly EndDate,
        Guid? BudgetId,
        Guid? CategoryId,
        Guid? BudgetCategoryId,
        Guid? AccountId,
        int Page = 0,
        int PageSize = 0
    );
}

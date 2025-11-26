namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record GetLedgerTransactionStatisticsRequest(
        Guid UserId,
        Guid LedgerId,
        DateOnly StartDate,
        DateOnly EndDate,
        Guid? BudgetId);
} 
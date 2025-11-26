namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record class GetCreateTransactionPageDataRequest(Guid? ledgerId, Guid? budgetId);
}

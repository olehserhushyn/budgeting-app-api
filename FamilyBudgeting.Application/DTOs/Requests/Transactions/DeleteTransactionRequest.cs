namespace FamilyBudgeting.Domain.DTOs.Requests.Transactions
{
    public record DeleteTransactionRequest(Guid TransactionId, Guid LedgerId);
}

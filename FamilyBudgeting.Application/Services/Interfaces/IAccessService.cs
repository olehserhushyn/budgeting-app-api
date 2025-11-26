namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IAccessService
    {
        Task<bool> UserHasAccessToLedgerAsync(Guid userId, Guid ledgerId);
        Task<bool> UserHasAccessToAccountAsync(Guid userId, Guid accountId);
        Task<bool> UserHasAccessToTransactionAsync(Guid userId, Guid transactionId);
        Task<bool> UserHasAccessToBudgetAsync(Guid userId, Guid budgetId);
        Task<bool> UserHasAccessToBudgetCategoryAsync(Guid userId, Guid budgetId);
    }
}

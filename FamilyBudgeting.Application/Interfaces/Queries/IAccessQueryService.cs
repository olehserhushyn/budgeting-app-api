namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IAccessQueryService
    {
        Task<bool> UserHasAccessToLedgerAsync(Guid userId, Guid ledgerId);
        Task<bool> UserHasAccessToAccountAsync(Guid userId, Guid accountId);
        Task<bool> UserHasAccessToTransactionAsync(Guid userId, Guid transactionId);
        Task<bool> UserHasAccessToBudgetAsync(Guid userId, Guid budgetId);
        Task<bool> UserHasAccessToBudgetCategoryAsync(Guid userId, Guid bCategoryId);
        Task<bool> UserHasLedgerRolesAsync(Guid userId, Guid ledgerId, params string[] ledgerRoleTitle);
    }
}

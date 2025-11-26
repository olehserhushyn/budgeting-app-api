using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class AccessService : IAccessService
    {
        private readonly IAccessQueryService _accessQueryService;

        public AccessService(IAccessQueryService accessQueryService)
        {
            _accessQueryService = accessQueryService;
        }

        public async Task<bool> UserHasAccessToLedgerAsync(Guid userId, Guid ledgerId)
        {
            return await _accessQueryService.UserHasAccessToLedgerAsync(userId, ledgerId);
        }

        public async Task<bool> UserHasAccessToAccountAsync(Guid userId, Guid accountId)
        {
            return await _accessQueryService.UserHasAccessToAccountAsync(userId, accountId);
        }

        public async Task<bool> UserHasAccessToTransactionAsync(Guid userId, Guid transactionId)
        {
            return await _accessQueryService.UserHasAccessToTransactionAsync(userId, transactionId);
        }

        public async Task<bool> UserHasAccessToBudgetAsync(Guid userId, Guid budgetId)
        {
            return await _accessQueryService.UserHasAccessToBudgetAsync(userId, budgetId);
        }

        public async Task<bool> UserHasAccessToBudgetCategoryAsync(Guid userId, Guid budgetId)
        {
            return await _accessQueryService.UserHasAccessToBudgetCategoryAsync(userId, budgetId);
        }
    }
}

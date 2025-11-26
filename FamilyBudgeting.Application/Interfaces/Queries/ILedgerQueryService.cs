using FamilyBudgeting.Domain.DTOs.Models.Ledgers;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface ILedgerQueryService
    {
        Task<IEnumerable<LedgerDto>> GetUserLedgersAsync(Guid userId);
        Task<LedgerDto?> GetUserLedgerFirstAsync(Guid userId);
        Task<LedgerDto?> GetUserLedgerFirstAsync(Guid userId, Guid budgetId);
    }
}

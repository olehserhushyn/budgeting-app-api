using FamilyBudgeting.Domain.Data.UserLedgers;

namespace FamilyBudgeting.Domain.Data.Ledgers
{
    public interface ILedgerRepository
    {
        Task<(Guid LedgerId, Guid UserLedgerId)> CreateLedgerAsync(Ledger ledger, UserLedger uLedger);
        Task<bool> UpdateLedgerAsync(Ledger ledger);
    }
}

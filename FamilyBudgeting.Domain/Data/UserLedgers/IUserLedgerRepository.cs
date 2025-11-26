namespace FamilyBudgeting.Domain.Data.UserLedgers
{
    public interface IUserLedgerRepository
    {
        Task<Guid> CreateUserLedgerAsync(UserLedger uLedger);
        Task<bool> UpdateUserLedgerAsync(Guid id, UserLedger uLedger);
    }
}

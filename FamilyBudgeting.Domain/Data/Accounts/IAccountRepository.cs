namespace FamilyBudgeting.Domain.Data.Accounts
{
    public interface IAccountRepository
    {
        Task<Guid> CreateAccountAsync(Account account);
        Task<bool> UpdateAccountAsync(Guid Id, Account account);
    }
}

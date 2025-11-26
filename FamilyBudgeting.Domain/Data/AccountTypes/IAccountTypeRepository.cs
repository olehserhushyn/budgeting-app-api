namespace FamilyBudgeting.Domain.Data.AccountTypes
{
    public interface IAccountTypeRepository
    {
        Task<Guid> CreateAccountTypeAsync(AccountType accountType);
    }
}

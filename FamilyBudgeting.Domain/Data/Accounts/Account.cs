namespace FamilyBudgeting.Domain.Data.Accounts
{
    public class Account : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid AccountTypeId { get; private set; }
        public string Title { get; private set; }
        public int Balance { get; private set; }
        public Guid CurrencyId { get; private set; }

        public Account(Guid userId, Guid accountTypeId, string title, int balance, Guid currencyId)
        {
            UserId = userId;
            AccountTypeId = accountTypeId;
            Title = title;
            Balance = balance;
            CurrencyId = currencyId;
        }

        public void Update(Guid accountTypeId, string title, int balance, Guid currencyId)
        {
            AccountTypeId = accountTypeId;
            Title = title;
            Balance = balance;
            CurrencyId = currencyId;

            UpdatedAt = DateTime.UtcNow;
        }

        public void AddTransaction(int amount)
        {
            Balance += amount;

            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTransaction(int oldAmount, int newAmount)
        {
            Balance += oldAmount;
            Balance += newAmount;

            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveTransaction(int amount)
        {
            Balance += amount;

            UpdatedAt = DateTime.UtcNow;
        }
    }
}

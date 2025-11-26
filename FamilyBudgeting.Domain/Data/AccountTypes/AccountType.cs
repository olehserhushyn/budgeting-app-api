namespace FamilyBudgeting.Domain.Data.AccountTypes
{
    public class AccountType
    {
        public Guid Id { get; init; }
        public string Title { get; private set; }

        public AccountType(string title)
        {
            Title = title;
        }
    }
}

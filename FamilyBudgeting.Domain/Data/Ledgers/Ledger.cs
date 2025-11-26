namespace FamilyBudgeting.Domain.Data.Ledgers
{
    public class Ledger : BaseEntity
    {
        public string? Title { get; private set; }

        public Ledger(string? title)
        {
            Title = title;
        }

        public void ChangeTitle(string? title)
        {
            Title = title;
        }
    }
}

namespace FamilyBudgeting.Domain.Data.TransactionTypes
{
    public class TransactionType
    {
        public Guid Id { get; private set; }
        public Guid Title { get; private set; }

        public TransactionType(Guid id, Guid title)
        {
            Id = id;
            Title = title;
        }
    }
}

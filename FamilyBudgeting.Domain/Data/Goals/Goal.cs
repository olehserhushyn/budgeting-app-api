namespace FamilyBudgeting.Domain.Data.Goals
{
    public class Goal : BaseEntity
    {
        public Guid LedgerId { get; private set; }
        public string Title { get; private set; }
        public int TargetAmount { get; private set; }
        public int CurrentAmount { get; private set; }
        public DateTime Deadline { get; private set; }
        public Guid CurrencyId { get; private set; }

        public Goal(Guid ledgerId, string title, int targetAmount, int currentAmount, DateTime deadline, Guid currencyId)
        {
            LedgerId = ledgerId;
            Title = title;
            TargetAmount = targetAmount;
            CurrentAmount = currentAmount;
            Deadline = deadline;
            CurrencyId = currencyId;
        }
    }
}

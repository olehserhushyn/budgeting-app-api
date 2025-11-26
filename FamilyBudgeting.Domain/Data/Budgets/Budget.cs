namespace FamilyBudgeting.Domain.Data.Budgets
{
    public class Budget : BaseEntity
    {
        public Guid LedgerId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public string? Title { get; private set; }

        public Budget(Guid ledgerId, DateTime startDate, DateTime endDate, string? title)
        {
            LedgerId = ledgerId;
            StartDate = startDate;
            EndDate = endDate;
            Title = title;
        }

        public void UpdateDetails(Guid ledgerId, DateTime startDate, DateTime endDate, string? title)
        {
            LedgerId = ledgerId;
            StartDate = startDate;
            EndDate = endDate;
            Title = title;
        }

        public void Delete()
        {
            IsDeleted = true;
            UpdatedAt = DateTime.Now;
        }
    }
}

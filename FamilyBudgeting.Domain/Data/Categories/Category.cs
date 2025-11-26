namespace FamilyBudgeting.Domain.Data.Categories
{
    public class Category : BaseEntity
    {
        public string Title{ get; private set; }
        public Guid LedgerId { get; private set; }
        public Guid TransactionTypeId { get; private set; }

        public Category(string title, Guid ledgerId, Guid transactionTypeId)
        {
            Title = title;
            LedgerId = ledgerId;
            TransactionTypeId = transactionTypeId;
        }

        public void UpdateDetails(string title, Guid ledgerId, Guid transactionTypeId)
        {
            Title = title;
            LedgerId = ledgerId;
            TransactionTypeId = transactionTypeId;
        }

        public void Delete()
        {
            this.IsDeleted = true;
            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}

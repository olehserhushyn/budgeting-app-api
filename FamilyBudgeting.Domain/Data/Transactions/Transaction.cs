namespace FamilyBudgeting.Domain.Data.Transactions
{
    public class Transaction : BaseEntity
    {
        public Guid AccountId { get; private set; }
        public Guid LedgerId { get; private set; }
        public Guid TransactionTypeId { get; private set; }
        public Guid? CategoryId { get; private set; }
        public Guid CurrencyId { get; private set; }
        public Guid? BudgetId { get; private set; }
        public Guid UserId { get; private set; }
        public int Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string? Note { get; private set; }
        public Guid? BudgetCategoryId { get; private set; }

        public Transaction(Guid accountId, Guid ledgerId, Guid transactionTypeId, Guid? categoryId,
            Guid currencyId, int amount, DateTime date, string? note, Guid? budgetId, Guid userId, 
            Guid? budgetCategoryId)
        {
            AccountId = accountId;
            LedgerId = ledgerId;
            TransactionTypeId = transactionTypeId;
            CategoryId = categoryId;
            CurrencyId = currencyId;
            Amount = amount;
            Date = date;
            Note = note;
            BudgetId = budgetId;
            UserId = userId;
            BudgetCategoryId = budgetCategoryId;
        }
    }
}

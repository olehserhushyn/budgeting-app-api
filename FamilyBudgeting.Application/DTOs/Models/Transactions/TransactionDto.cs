namespace FamilyBudgeting.Domain.DTOs.Models.Transactions
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid AccountId { get; set; }
        public Guid LedgerId { get; set; }
        public Guid TransactionTypeId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid BudgetCategoryId { get; set; }
        public Guid CurrencyId { get; set; }
        public Guid BudgetId { get; set; }
        public Guid UserId { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }
}

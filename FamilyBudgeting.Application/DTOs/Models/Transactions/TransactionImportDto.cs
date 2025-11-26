namespace FamilyBudgeting.Domain.DTOs.Models.Transactions
{
    public class TransactionImportDto
    {
        public string AccountName { get; set; }
        public string TransactionType { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
    }
}

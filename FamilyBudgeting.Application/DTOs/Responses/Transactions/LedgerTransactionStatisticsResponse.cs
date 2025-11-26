namespace FamilyBudgeting.Domain.DTOs.Responses.Transactions
{
    public class LedgerTransactionStatisticsResponse
    {
        public decimal FormattedTotalIncome { get; set; }
        public decimal FormattedTotalExpenses { get; set; }
        public int TransactionCount { get; set; }
        public List<CategoryBreakdown> CategoryBreakdowns { get; set; } = new();
        public List<TransactionTypeBreakdown> TransactionTypeBreakdowns { get; set; } = new();
    }

    public class CategoryBreakdown
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal FormattedTotalAmount { get; set; }
        public int TransactionCount { get; set; }

        public string TransactionTypeName { get; set; }
    }

    public class TransactionTypeBreakdown
    {
        public Guid TransactionTypeId { get; set; }
        public string TransactionTypeName { get; set; }
        public decimal FormattedTotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }
} 
namespace FamilyBudgeting.Domain.DTOs.Responses.Transactions
{
    public class GetCreateTransactionPageDataResponse
    {
        public Dictionary<Guid, string> Accounts { get; set; } = new Dictionary<Guid, string>();
        //public Dictionary<Guid, string> Ledgers { get; set; }
        public Dictionary<Guid, string> TransactionTypes { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, CategoryWithTypeDto> TransactionCategories { get; set; } = new Dictionary<Guid, CategoryWithTypeDto>();
        public Dictionary<Guid, CategoryWithTypeDto> BudgetCategories { get; set; } = new Dictionary<Guid, CategoryWithTypeDto>();
        public Dictionary<Guid, string> Currencies { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Budgets { get; set; } = new Dictionary<Guid, string>();
    }

    public class CategoryWithTypeDto
    {
        public string Title { get; set; }
        public Guid TransactionTypeId { get; set; }
        public string TransactionTypeTitle { get; set; }
    }
}

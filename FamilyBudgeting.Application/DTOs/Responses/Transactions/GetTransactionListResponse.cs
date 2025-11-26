using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyBudgeting.Domain.DTOs.Responses.Transactions
{
    public class GetTransactionListResponse
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string AccountTitle { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; }
        public Guid LedgerId { get; set; }
        public Guid TransactionTypeId { get; set; }
        public string TransactionTypeName { get; set; }
        public Guid BudgetCategoryId { get; set; }
        public string BudgetCategoryName { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid CurrencyId { get; set; }
        public string CurrencyTitle { get; set; }
        public int CurrencyFractionalUnitFactor { get; set; }
        public Guid? BudgetId { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }

        [NotMapped]
        public double FormattedAmount => (double)this.Amount / this.CurrencyFractionalUnitFactor;
    }

    public class GetTransactionListResponse_Summary
    {
        public int TotalAmountIncome { get; set; }
        public int TotalAmountExpense { get; set; }
        public int TotalAmountNet { get; set; }
        public Guid CurrencyId { get; set; }
        public string CurrencyTitle { get; set; }
        public int CurrencyFractionalUnitFactor { get; set; }

        [NotMapped]
        public double FormattedTotalAmountIncome => (double)this.TotalAmountIncome / this.CurrencyFractionalUnitFactor;
        public double FormattedTotalAmountExpense => (double)this.TotalAmountExpense / this.CurrencyFractionalUnitFactor;
        public double FormattedTotalAmountNet => (double)this.TotalAmountNet / this.CurrencyFractionalUnitFactor;
    }
}

public class PaginatedTransactionListResponse
{
    public IEnumerable<GetTransactionListResponse> Items { get; set; }
    public GetTransactionListResponse_Summary Summary { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

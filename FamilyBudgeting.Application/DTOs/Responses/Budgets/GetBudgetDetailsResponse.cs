using FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyBudgeting.Domain.DTOs.Responses.Budgets
{
    public class GetBudgetDetailsResponse
    {
        public Guid BudgetId { get; set; }
        public DateTime BudgetStartDate { get; set; }
        public DateTime BudgetEndDate { get; set; }
        public string? BudgetTitle { get; set; }

        public List<BudgetCategoriesDetailedResponse> BudgetCategories { get; set; } = new List<BudgetCategoriesDetailedResponse>();
        public int ExpenseTotalPlannedAmount { get; set; }
        public int ExpenseTotalSpentAmount { get; set; }
        public int IncomeTotalPlannedAmount { get; set; }
        public int IncomeTotalSpentAmount { get; set; }

        [NotMapped]
        public int CurrencyFractionalUnitFactor { get; set; } = 100;

        // Formatted Properties
        [NotMapped]
        public double FormattedExpenseTotalPlannedAmount =>
            (double)ExpenseTotalPlannedAmount / CurrencyFractionalUnitFactor;

        [NotMapped]
        public double FormattedExpenseTotalSpentAmount =>
            (double)ExpenseTotalSpentAmount / CurrencyFractionalUnitFactor;

        [NotMapped]
        public double FormattedIncomeTotalPlannedAmount =>
            (double)IncomeTotalPlannedAmount / CurrencyFractionalUnitFactor;

        [NotMapped]
        public double FormattedIncomeTotalSpentAmount =>
            (double)IncomeTotalSpentAmount / CurrencyFractionalUnitFactor;
    }
}

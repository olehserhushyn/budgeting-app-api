using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories
{
    public class GetBudgetCategoriesDetailedResponse
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int PlannedAmount { get; set; }
        public int InitialPlannedAmount { get; set; }
        public int CurrentAmount { get; set; }
        public Guid CurrencyId { get; set; }
        public string CurrencyCode { get; set; }  // ISO 4217 currency code (e.g., USD, EUR)
        public string CurrencyName { get; set; }  // Full currency name (e.g., US Dollar)
        public string? CurrencySymbol { get; set; }  // Currency symbol (e.g., $, €, £)
        public int CurrencyFractionalUnitFactor { get; set; } // Nullable Exchange Rate
        public Guid TransactionTypeId { get; set; }
        public string TransactionTypeTitle { get; set; }

        [NotMapped]
        public double FormattedPlannedAmount => CurrencyFractionalUnitFactor <= 0 ? 0 : (double)PlannedAmount / this.CurrencyFractionalUnitFactor;

        [NotMapped]
        public double FormattedCurrentAmount => CurrencyFractionalUnitFactor <= 0 ? 0 : (double)CurrentAmount / this.CurrencyFractionalUnitFactor;

        //[NotMapped]
        //public double FormattedSpentAmount => CurrencyFractionalUnitFactor <= 0 ? 0 : (double)SpentAmount / this.CurrencyFractionalUnitFactor;
    }
}
